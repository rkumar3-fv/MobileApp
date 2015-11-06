using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Faxes uploading service
    /// Used in foreground mode
    /// </summary>
    [Service(Exported = false)]
    public class AttachmentsDownloadService : Service
    {
        public const string ActionIdTag = "ActionIdTag";
        public const string ActionMsgTag = "ActionMsgTag";
        public const string ActionNotificationTag = "ActionNotificationTag";
        public const string ActionStartTag = "ActionTypeStart";
        public const string ActionStopTag = "ActionTypeStop";
        public const string ActionStatusTag = "ActionTypeState";

        private ResultReceiver _receiver;
        private bool _isInWork;
        private ConcurrentQueue<Tuple<int, string, string>> _downloadingUrls;
        private ConcurrentDictionary<int, CancellationToken> _cancellationTokens;

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
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (_receiver == null)
                _receiver = intent.GetParcelableExtra(AttachmentsServiceResultReceiver.ReceiverTag) as ResultReceiver;
            var id = intent.GetIntExtra(ActionIdTag, 0);
            switch (intent.Action)
            {
                case ActionStartTag:
                    break;

                case ActionStopTag:
                    break;

                case ActionStatusTag:

                    break;
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
            var lastPart = url.Split('/').Last().ToLower();
            var res = ApiHelper.MakeAsyncFileDownload(url, "application/json", token).Result;
            if (res.Code != ErrorCodes.Ok)
            {
                _isInWork = false;
                return;
            }
            using (var ms = new MemoryStream())
            {
                res.Result.CopyTo(ms);
                var bytes = ms.ToArray();
                using (var file = new FileStream($"{root}{path}.{lastPart}", FileMode.Create, FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }
            }
            _isInWork = false;
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