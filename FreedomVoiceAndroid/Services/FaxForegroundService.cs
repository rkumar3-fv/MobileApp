using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Faxes uploading service
    /// Used in foreground mode
    /// </summary>
    [Service(Exported = false)]
    public class FaxForegroundService : Service
    {
        public const string ActionTag = "ExtraAction";
        public const string ActionIdTag = "ActionIdTag";
        public const string ActionExecuteTag = "ActionTypeExecute";
        public const string ActionStopTag = "ActionTypeStop";
        public const string ActionStatusTag = "ActionTypeState";
        private bool _isInWork;
        private ResultReceiver _receiver;
        private ConcurrentQueue<Pair> _downloadingUrls;
        private ConcurrentDictionary<long, CancellationToken> _cancellationTokens;

        public event EventHandler<int> ProgressEvent;
        public event EventHandler<string> SuccessEvent;

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Log.Debug(App.AppPackage, "DOWNLOADING FOREGROUND SERVICE STARTED");
#endif
            _downloadingUrls = new ConcurrentQueue<Pair>();
            _cancellationTokens = new ConcurrentDictionary<long, CancellationToken>();
        }

        public override IBinder OnBind(Intent intent)
        {
            var binder = new FaxForegroundBinder(this);
            return binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var id = intent.GetLongExtra(ActionIdTag, 0);
            var requestUrl = intent.GetStringExtra(ActionTag);
            if (requestUrl == null) return StartCommandResult.NotSticky;
#if DEBUG
            Log.Debug(App.AppPackage, $"DOWNLOADING ID={id}, DOWNLOAD URL={requestUrl}");
#endif
            var token = new CancellationToken();
            _downloadingUrls.Enqueue(new Pair(id, requestUrl));
            _cancellationTokens.TryAdd(id, token);
            if (!_isInWork)
                Start();
            return StartCommandResult.NotSticky;
        }

        private void Start()
        {
            if (!_downloadingUrls.IsEmpty)
            {
                if (_isInWork)
                    Start();
                Pair item;
                if (!_downloadingUrls.TryDequeue(out item))
                    Start();
                CancellationToken token;
                if (!_cancellationTokens.TryGetValue((long) item.First, out token))
                    Start();
                _isInWork = true;
                Task.Factory.StartNew(() => BackgroundDownloading((string) item.Second, token), token).ContinueWith(
                    (t) =>
                    {
                        CancellationToken removingToken;
                        _cancellationTokens.TryRemove((long) item.First, out removingToken);
                        Start();
                    }, token);
            }
            else
            {
                _isInWork = false;
                StopSelf();
            }
        }

        private void BackgroundDownloading(string url, CancellationToken token)
        {
            var root = $"{CacheDir.AbsolutePath}/";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var path = Path.GetRandomFileName();
            var res = ApiHelper.MakeAsyncFileDownload(url, "application/json", token).Result;
            using (var ms = new MemoryStream())
            {
                res.Result.CopyTo(ms);
                var bytes = ms.ToArray();
                using (var file = new FileStream($"{root}\\{path}.pdf", FileMode.Create, FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }
            }
            _isInWork = false;
            SuccessEvent?.Invoke(this, $"{root}{path}");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE DESTROYED");
#endif
            _receiver = null;
        }
    }
}