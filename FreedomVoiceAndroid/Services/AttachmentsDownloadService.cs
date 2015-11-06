using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Actions.Reports;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Entities.EventArgs;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;

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
        private ConcurrentQueue<Message> _downloadingUrls;
        private ConcurrentDictionary<int, CancellationToken> _cancellationTokens;

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Log.Debug(App.AppPackage, "DOWNLOADING FOREGROUND SERVICE STARTED");
#endif
            _downloadingUrls = new ConcurrentQueue<Message>();
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
                    var msg = intent.GetParcelableExtra(ActionMsgTag) as Message;
                    if (msg != null)
                    {
                        var token = new CancellationToken();
                        _cancellationTokens.TryAdd(msg.Id, token);
                        _downloadingUrls.Enqueue(msg);
                    }
                    if (!_isInWork)
                        Start();
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
                Message item;
                if (!_downloadingUrls.TryDequeue(out item))
                    Start();
                CancellationToken token;
                if (!_cancellationTokens.TryGetValue(item.Id, out token))
                    Start();
                _isInWork = true;
                Task.Factory.StartNew(() => BackgroundDownloading(item, token), token).ContinueWith(
                    t => {
                        CancellationToken removingToken;
                        _cancellationTokens.TryRemove(item.Id, out removingToken);
                        Start();
                    }, token);
            }
            else
            {
                _isInWork = false;
                StopSelf();
            }
        }

        private void BackgroundDownloading(Message msg, CancellationToken token)
        {
            var root = $"{GetExternalFilesDir(null)}/";
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);
            var path = Path.GetRandomFileName();
            var lastPart = msg.AttachUrl.Split('/').Last().ToLower();
            ApiHelper.OnDownloadStatus += ApiHelperOnDownloadStatus;
            var res = ApiHelper.MakeAsyncFileDownload(msg.AttachUrl, "application/json", msg.Id, token).Result;
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
#if DEBUG
                Log.Debug(App.AppPackage, $"LOADED {root}{path}.{lastPart}");
#endif
            }
            ApiHelper.OnDownloadStatus -= ApiHelperOnDownloadStatus;
            _isInWork = false;
        }

        /// <summary>
        /// Progress event
        /// </summary>
        /// <param name="messageId">message ID</param>
        /// <param name="args">progress args</param>
        private void ApiHelperOnDownloadStatus(string messageId, DownloadStatusArgs args)
        {
            var data = new Bundle();
            int id;
            int.TryParse(messageId, out id);
            data.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra, new ProgressReport(id, args.Progress));
#if DEBUG
            Log.Debug(App.AppPackage, $"ATTACHMENT {messageId} PROGRESS = {args.Progress}%");
#endif
            _receiver.Send(Result.Ok, data);
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