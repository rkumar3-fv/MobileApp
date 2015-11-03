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
        public const string ActionNameTag = "ActionNameTag";
        public const string ActionExecuteTag = "ActionTypeExecute";
        public const string ActionStopTag = "ActionTypeStop";
        public const string ActionStatusTag = "ActionTypeState";
        private bool _isInWork;
        private ConcurrentQueue<Tuple<int, string, string>> _downloadingUrls;
        private ConcurrentDictionary<int, CancellationToken> _cancellationTokens;

        public event ProgressEventHandler ProgressEvent;
        public event SuccessEventHandler SuccessEvent;
        public event StartLoadingEventHandler StartEvent;

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Log.Debug(App.AppPackage, "DOWNLOADING FOREGROUND SERVICE STARTED");
#endif
            _downloadingUrls = new ConcurrentQueue<Tuple<int, string, string>>();
            _cancellationTokens = new ConcurrentDictionary<int, CancellationToken>();
        }

        public override IBinder OnBind(Intent intent)
        {
            var binder = new FaxForegroundBinder(this);
            return binder;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var id = intent.GetIntExtra(ActionIdTag, 0);
            var name = intent.GetStringExtra(ActionNameTag);
            var requestUrl = intent.GetStringExtra(ActionTag);
            if (requestUrl == null) return StartCommandResult.NotSticky;
#if DEBUG
            Log.Debug(App.AppPackage, $"DOWNLOADING ID={id}, DOWNLOAD URL={requestUrl}");
#endif
            var token = new CancellationToken();
            _downloadingUrls.Enqueue(new Tuple<int, string, string>(id, name, requestUrl));
            _cancellationTokens.TryAdd(id, token);
            if (!_isInWork)
            {
                Start();
            }
            return StartCommandResult.NotSticky;
        }

        private void Start()
        {
            if (!_downloadingUrls.IsEmpty)
            {
                if (_isInWork)
                    Start();
                Tuple<int, string, string> item;
                if (!_downloadingUrls.TryDequeue(out item))
                    Start();
                CancellationToken token;
                if (!_cancellationTokens.TryGetValue(item.Item1, out token))
                    Start();
                _isInWork = true;
                Task.Factory.StartNew(() => BackgroundDownloading(item.Item1, item.Item2, item.Item3, token), token).ContinueWith(
                    t => {
                        CancellationToken removingToken;
                        _cancellationTokens.TryRemove(item.Item1, out removingToken);
                        Start();
                    }, token);
            }
            else
            {
                _isInWork = false;
                StopSelf();
            }
        }

        private void BackgroundDownloading(int id, string name, string url, CancellationToken token)
        {
            var root = $"{GetExternalFilesDir(null)}/";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var path = Path.GetRandomFileName();
            var res = ApiHelper.MakeAsyncFileDownload(url, "application/json", token).Result;
            if (res.Code != ErrorCodes.Ok)
            {
                _isInWork = false;
                return;
            }
            StartEvent?.Invoke(this, new AttachmentHelperEventArgs<string>(id, name));
            using (var ms = new MemoryStream())
            {
                res.Result.CopyTo(ms);
                var bytes = ms.ToArray();
                using (var file = new FileStream($"{root}{path}.pdf", FileMode.Create, FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }
            }
            _isInWork = false;
            SuccessEvent?.Invoke(this, new AttachmentHelperEventArgs<string>(id, $"{root}{path}.pdf"));
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE DESTROYED");
#endif
        }
    }
}