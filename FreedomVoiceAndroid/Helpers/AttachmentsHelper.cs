using System.Collections.Generic;
using System.IO;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Actions.Reports;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Services;
using FreedomVoice.Core.Utils;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;
using NotificationCompat = Android.Support.V7.App.NotificationCompat;
using TaskStackBuilder = Android.App.TaskStackBuilder;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public delegate void SuccessEventHandler(object sender, AttachmentHelperEventArgs<string> args);

    public delegate void ProgressLoadingEventHandler(object sender, AttachmentHelperEventArgs<int> args);

    public delegate void StopLoadingEventHandler(object sender, AttachmentHelperEventArgs<bool> args);

    /// <summary>
    /// Attachments managment
    /// </summary>
    public class AttachmentsHelper : IAppServiceResultReceiver
    {
        public const int AttachmentActionNotificationId = 102;
        private readonly Context _context;
        private readonly Dictionary<int, string> _cacheDictionary;
        private readonly List<int> _waitingList;
        private readonly NotificationCompat.Builder _builder;
        private readonly NotificationManagerCompat _notificationManager;

        /// <summary>
        /// Result receiver for service communication
        /// </summary>
        private readonly ComServiceResultReceiver _receiver;

        public event SuccessEventHandler OnFinish;
        public event StopLoadingEventHandler FailLoadingEvent;
        public event ProgressLoadingEventHandler OnProgressLoading;

        public AttachmentsHelper(Context context)
        {
            _context = context;
            _cacheDictionary = new Dictionary<int, string>();
            _waitingList = new List<int>();
            _receiver = new ComServiceResultReceiver(new Handler());
            _receiver.SetListener(this);

            _notificationManager = NotificationManagerCompat.From(_context);
            _builder = new NotificationCompat.Builder(_context);
            _builder.SetCategory(Notification.CategoryTransport);
            _builder.SetOngoing(false);
            _builder.SetProgress(0, 0, false);
            _builder.SetAutoCancel(true);
        }

        public long LoadAttachment(Message msg)
        {
            if (_cacheDictionary.ContainsKey(msg.Id))
            {
#if DEBUG
                var files = Directory.GetFiles("/storage/emulated/0/Android/data/com.FreedomVoice.MobileApp.Android/files/");
                foreach (var file in files)
                {
                    Log.Debug(App.AppPackage, "IN DIRECTORY: " + file);
                }
#endif
                if (File.Exists(_cacheDictionary[msg.Id]))
                {
#if DEBUG
                    Log.Debug(App.AppPackage, "FILE ALREADY DOWNLOADED: " + _cacheDictionary[msg.Id]);
#endif
                    OnFinish?.Invoke(this, new AttachmentHelperEventArgs<string>(msg.Id, msg.MessageType, _cacheDictionary[msg.Id]));
                    return msg.Id;
                }
            }
            var intent = new Intent(_context, typeof(AttachmentsDownloadService));
            intent.PutExtra(AttachmentsServiceResultReceiver.ReceiverTag, _receiver);
            if (_waitingList.Contains(msg.Id))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "FILE ALREADY DOWNLOADING: " +msg.Id);
#endif
                intent.SetAction(AttachmentsDownloadService.ActionStatusTag);
            }
            else
            {
                _waitingList.Add(msg.Id);
                intent.SetAction(AttachmentsDownloadService.ActionStartTag);
                intent.PutExtra(AttachmentsDownloadService.ActionIdTag, msg.Id);
                intent.PutExtra(AttachmentsDownloadService.ActionMsgTag, msg);
            }
            _context.StartService(intent);
            return msg.Id;
        }

        /// <summary>
        /// Receive result from service
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="resultData"></param>
        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            if (resultCode != (int)Result.Ok) return;
            var report = resultData.GetParcelable(AttachmentsServiceResultReceiver.ReceiverDataExtra) as BaseReport;
            if (report != null)
                OnReceiveResult(report);
        }

        private void OnReceiveResult(BaseReport report)
        {
            var type = report.GetType().Name;
            switch (type)
            {
                case "ErrorReport":
                    var errorReport = (ErrorReport) report;
                    _waitingList.Remove(report.Id);
                    if (errorReport.ErrorCode != ErrorReport.ErrorCancelled)
                    {
                        _builder.SetContentText(DataFormatUtils.ToPhoneNumber(report.Msg.FromNumber));
                        string title;
                        int icon;
                        switch (report.Msg.MessageType)
                        {
                            case Message.TypeFax:
                                title = _context.GetString(Resource.String.Notif_fax_fail);
                                icon = Resource.Drawable.ic_notification_fax;
                                break;
                            case Message.TypeRec:
                                title = _context.GetString(Resource.String.Notif_record_fail);
                                icon = Resource.Drawable.ic_notification_playback;
                                break;
                            case Message.TypeVoice:
                                title = _context.GetString(Resource.String.Notif_voicemail_fail);
                                icon = Resource.Drawable.ic_notification_playback;
                                break;
                            default:
                                title = _context.GetString(Resource.String.Notif_attachment_fail);
                                icon = Resource.Drawable.ic_notification_download;
                                break;
                        }
                        _builder.SetSmallIcon(icon);
                        _builder.SetContentTitle(title);
                        _notificationManager.Notify(AttachmentActionNotificationId, _builder.Build());
                        FailLoadingEvent?.Invoke(this, new AttachmentHelperEventArgs<bool>(errorReport.Id, errorReport.Msg.MessageType, false));
                    }
                    else
                        FailLoadingEvent?.Invoke(this, new AttachmentHelperEventArgs<bool>(errorReport.Id, errorReport.Msg.MessageType, true));
                    break;
                case "ProgressReport":
                    var progressReport = (ProgressReport)report;
                    if (!_waitingList.Contains(progressReport.Id))
                        _waitingList.Add(progressReport.Id);
                    OnProgressLoading?.Invoke(this, new AttachmentHelperEventArgs<int>(progressReport.Id, progressReport.Msg.MessageType, progressReport.ProgressValue));
                    break;
                case "SuccessReport":
                    var successReport = (SuccessReport)report;
                    _waitingList.Remove(successReport.Id);
                    _cacheDictionary.Add(successReport.Id, successReport.Path);
                    if ((OnFinish == null) || (OnFinish.GetInvocationList().Length == 0))
                    {
                        _builder.SetContentText(DataFormatUtils.ToPhoneNumber(report.Msg.FromNumber));
                        string title;
                        int icon;
                        Intent intent;
                        switch (report.Msg.MessageType)
                        {
                            case Message.TypeFax:
                                title = _context.GetString(Resource.String.Notif_fax_success);
                                icon = Resource.Drawable.ic_notification_fax;

                                intent = new Intent(Intent.ActionView);
                                var file = new Java.IO.File(successReport.Path);
                                file.SetReadable(true);
                                intent.SetDataAndType(Uri.FromFile(file), "application/pdf");
                                intent.SetFlags(ActivityFlags.NoHistory);
                                var resultPendingIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.CancelCurrent);
                                _builder.SetContentIntent(resultPendingIntent);
                                break;
                            case Message.TypeRec:
                                title = _context.GetString(Resource.String.Notif_record_success);
                                icon = Resource.Drawable.ic_notification_playback;

                                intent = new Intent(_context, typeof(VoiceRecordActivity));
                                intent.SetFlags(ActivityFlags.NoHistory);
                                intent.PutExtra(MessageDetailsActivity.MessageExtraTag, successReport.Msg);
                                var recordIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.CancelCurrent);
                                _builder.SetContentIntent(recordIntent);
                                break;
                            case Message.TypeVoice:
                                title = _context.GetString(Resource.String.Notif_voicemail_success);
                                icon = Resource.Drawable.ic_notification_playback;

                                intent = new Intent(_context, typeof(VoiceMailActivity));
                                intent.SetFlags(ActivityFlags.NoHistory);
                                intent.PutExtra(MessageDetailsActivity.MessageExtraTag, successReport.Msg);
                                var voiceIntent = PendingIntent.GetActivity(_context, 0, intent, PendingIntentFlags.CancelCurrent);
                                _builder.SetContentIntent(voiceIntent);
                                break;
                            default:
                                title = _context.GetString(Resource.String.Notif_attachment_success);
                                icon = Resource.Drawable.ic_notification_download;
                                break;
                        }
                        _builder.SetSmallIcon(icon);
                        _builder.SetContentTitle(title);
                        _notificationManager.Notify(AttachmentActionNotificationId, _builder.Build());
                    }
                    else
                    {
                        _notificationManager.Cancel(AttachmentActionNotificationId);
                        OnFinish.Invoke(this, new AttachmentHelperEventArgs<string>(successReport.Id, successReport.Msg.MessageType, successReport.Path));
                    }
                    break;
            }
        }
    }
}