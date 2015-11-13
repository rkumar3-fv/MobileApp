using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Actions.Reports;
using com.FreedomVoice.MobileApp.Android.Utils;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;
using NotificationCompat = Android.Support.V7.App.NotificationCompat;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Faxes uploading service
    /// Used in foreground mode
    /// </summary>
    [Service(Exported = false)]
    public class AttachmentsDownloadService : Service
    {
        private const int ProgressNotificationId = 101;
        private const int BufferSize = 4096;
        public const string ActionIdTag = "ActionIdTag";
        public const string ActionMsgTag = "ActionMsgTag";
        public const string ActionStartTag = "ActionTypeStart";
        public const string ActionStopTag = "ActionTypeStop";

        private ResultReceiver _receiver;
        private bool _isInWork;
        private ConcurrentQueue<Message> _downloadingUrls;
        private ConcurrentDictionary<int, CancellationTokenSource> _cancellationTokens;
        private NotificationCompat.Builder _builder;
        private NotificationManagerCompat _notificationManager;

        public override void OnCreate()
        {
            base.OnCreate();
#if DEBUG
            Log.Debug(App.AppPackage, "DOWNLOADING FOREGROUND SERVICE STARTED");
#endif
            _downloadingUrls = new ConcurrentQueue<Message>();
            _cancellationTokens = new ConcurrentDictionary<int, CancellationTokenSource>();

            _notificationManager = NotificationManagerCompat.From(this);
            _builder = new NotificationCompat.Builder(this);
            _builder.SetSmallIcon(Resource.Drawable.ic_notification_download);
            _builder.SetCategory(Notification.CategoryProgress);
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
                        var tokenSource = new CancellationTokenSource();
                        _cancellationTokens.TryAdd(msg.Id, tokenSource);
                        _downloadingUrls.Enqueue(msg);
                    }
                    if (!_isInWork)
                        Start();
                    break;

                case ActionStopTag:
                    if (_isInWork)
                    {
                        if (_cancellationTokens.ContainsKey(id))
                        {
                            CancellationTokenSource tokenSource;
                            var res = _cancellationTokens.TryRemove(id, out tokenSource);
                            if (res)
                                tokenSource.Cancel();
                        }
                    }
                    break;
            }
            return StartCommandResult.NotSticky;
        }

        private void Start()
        {
            if (_isInWork)
                return;
            if (!_downloadingUrls.IsEmpty)
            {
                Message item;
                if (!_downloadingUrls.TryDequeue(out item))
                    Start();
                CancellationTokenSource tokenSource;
                if (!_cancellationTokens.TryGetValue(item.Id, out tokenSource))
                    Start();
                _isInWork = true;

                string title;
                switch (item.MessageType)
                {
                    case Message.TypeFax:
                        title = GetString(Resource.String.Notif_fax_progress);
                        break;
                    case Message.TypeRec:
                        title = GetString(Resource.String.Notif_record_progress);
                        break;
                    case Message.TypeVoice:
                        title = GetString(Resource.String.Notif_voicemail_progress);
                        break;
                    default:
                        title = GetString(Resource.String.Notif_attachment_progress);
                        break;
                }
                _builder.SetContentTitle(title);
                string text;
                ContactsHelper.Instance(this).GetName(item.FromNumber, out text);
                _builder.SetContentText(text);
                _builder.SetProgress(100, 100, true);
                var notification = _builder.Build();
                
                StartForeground(ProgressNotificationId, notification);
                if (tokenSource != null)
                {
                    var token = tokenSource.Token;
                    Task.Factory.StartNew(() => LoadFile(item, token), token).ContinueWith(
                            t =>
                            {
                                CancellationTokenSource removingToken;
                                _cancellationTokens.TryRemove(item.Id, out removingToken);
                                _isInWork = false;
                                StopForeground(true);
                                Start();
                            }, token);
                }
                else
                    Start();
            }
            else
                StopSelf();
        }

        private void LoadFile(Message msg, CancellationToken token)
        {
            var rootDirectory = $"{GetExternalFilesDir(null)}/";
            try
            {
                if (!Directory.Exists(rootDirectory))
                    Directory.CreateDirectory(rootDirectory);
            }
            catch (Exception)
            {
#if DEBUG
                Log.Debug(App.AppPackage, "MEDIA REQUEST FAILED: UNABLE TO CREATE DIRECTORY");
#endif
                var failData = new Bundle();
                failData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra, new ErrorReport(msg.Id, msg, ErrorReport.ErrorBadRequest));
                _receiver.Send(Result.Ok, failData);
                return;
            }
            var fileExtension = msg.AttachUrl.Split('/').Last().ToLower();
            var fileName = Path.GetRandomFileName();
            var fullName = $"{rootDirectory}{fileName}.{fileExtension}";
            var lastProgress = 0;
            var totalRead = 0;
#if DEBUG
            Log.Debug(App.AppPackage, $"MEDIA REQUEST to {msg.AttachUrl}");
#endif
            var res = ApiHelper.MakeAsyncFileDownload(msg.AttachUrl, "application/json", token).Result;
            if (res == null)
            {
#if DEBUG
                Log.Debug(App.AppPackage, $"MEDIA REQUEST FAILED : CONNECTION LOST");
#endif
                var failData = new Bundle();
                failData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra, new ErrorReport(msg.Id, msg, ErrorCodes.ConnectionLost));
                _receiver.Send(Result.Ok, failData);
                return;
            }
            if (res.Code != ErrorCodes.Ok)
            {
#if DEBUG
                Log.Debug(App.AppPackage, $"MEDIA REQUEST FAILED with CODE = {res.Code}");
#endif
                var failData = new Bundle();
                failData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra, new ErrorReport(msg.Id, msg, res.Code));
                _receiver.Send(Result.Ok, failData);
                return;
            }
#if DEBUG
            Log.Debug(App.AppPackage, $"STREAM RESPONSE RECEIVED, LENGTH = {res.Result.Length}");
#endif
            var buffer = new byte[BufferSize];
            try
            {
                using (var fs = new FileStream(fullName, FileMode.Create, FileAccess.Write))
                {
                    int bytesRead;
#if DEBUG
                    Log.Debug(App.AppPackage, "START LOADING, PROGRESS IS 0%");
#endif
                    var progressData = new Bundle();
                    progressData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                        new ProgressReport(msg.Id, msg, 0));
                    _receiver.Send(Result.Ok, progressData);

                    while ((bytesRead = res.Result.ReceivedStream.Read(buffer, 0, BufferSize)) > 0)
                    {
                        if (token.IsCancellationRequested)
                        {
#if DEBUG
                            Log.Debug(App.AppPackage, "LOADING CANCELLED");
#endif
                            _builder.SetProgress(100, 0, false);
                            _notificationManager.Notify(ProgressNotificationId, _builder.Build());
                            var failData = new Bundle();
                            failData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                                new ErrorReport(msg.Id, msg, ErrorReport.ErrorCancelled));
                            _receiver.Send(Result.Ok, failData);
                            return;
                        }
                        totalRead += bytesRead;
                        fs.Write(buffer, 0, bytesRead);
                        var progress = Convert.ToInt32(totalRead*100/res.Result.Length);
                        if (progress > lastProgress)
                        {
#if DEBUG
                            Log.Debug(App.AppPackage, $"LOADING PROGRESS: {progress}%");
#endif
                            _builder.SetProgress(100, progress, false);
                            _notificationManager.Notify(ProgressNotificationId, _builder.Build());
                            progressData = new Bundle();
                            progressData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                                new ProgressReport(msg.Id, msg, progress));
                            _receiver.Send(Result.Ok, progressData);
                            lastProgress = progress;
                        }
                    }
                    var resData = new Bundle();
                    if (!token.IsCancellationRequested)
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, $"MEDIA FILE SAVED. Path - {fullName}");
#endif
                        resData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                            new SuccessReport(msg.Id, msg, fullName));
                    }
                    else
                    {
#if DEBUG
                        Log.Debug(App.AppPackage, "MEDIA FILE LOADING CANCELLED");
#endif
                        resData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                            new ErrorReport(msg.Id, msg, ErrorReport.ErrorCancelled));
                    }
                    _receiver.Send(Result.Ok, resData);
                }
            }
            catch (Exception)
            {
#if DEBUG
                Log.Debug(App.AppPackage, "MEDIA REQUEST FAILED: UNABLE TO WRITE FILE");
#endif
                var failData = new Bundle();
                failData.PutParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra,
                    new ErrorReport(msg.Id, msg, ErrorReport.ErrorBadRequest));
                _receiver.Send(Result.Ok, failData);
            }
            finally
            {
                res.Result.ReceivedStream?.Close();
            }
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