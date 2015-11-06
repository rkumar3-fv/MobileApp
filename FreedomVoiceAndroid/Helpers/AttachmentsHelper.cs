using System.Collections.Generic;
using System.IO;
using System.Linq;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Notifications;
using com.FreedomVoice.MobileApp.Android.Services;
using FreedomVoice.Core.Utils;
using Message = com.FreedomVoice.MobileApp.Android.Entities.Message;
using Uri = Android.Net.Uri;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public delegate void SuccessEventHandler(object sender, AttachmentHelperEventArgs<string> args);

    public delegate void StartLoadingEventHandler(object sender, AttachmentHelperEventArgs<string> args);

    public delegate void StopLoadingEventHandler(object sender, AttachmentHelperEventArgs<bool> args);

    /// <summary>
    /// Attachments managment
    /// </summary>
    public class AttachmentsHelper : IAppServiceResultReceiver
    {
        private readonly Context _context;
        private readonly Dictionary<int, string> _cacheDictionary;
        private readonly List<int> _waitingList;
        private readonly FaxDownloadNotification _faxNotification;

        /// <summary>
        /// Result receiver for service communication
        /// </summary>
        private readonly ComServiceResultReceiver _receiver;

        public event SuccessEventHandler OnFinish;
        public event StopLoadingEventHandler FailLoadingEvent;
        public event StartLoadingEventHandler StartLoadingEvent;

        public AttachmentsHelper(Context context)
        {
            _context = context;
            _cacheDictionary = new Dictionary<int, string>();
            _waitingList = new List<int>();
            _faxNotification = FaxDownloadNotification.Instance(_context);
            _receiver = new ComServiceResultReceiver(new Handler());
            _receiver.SetListener(this);
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
                    OnFinish?.Invoke(this, new AttachmentHelperEventArgs<string>(msg.Id, msg.AttachUrl.Split('/').Last().ToLower(), _cacheDictionary[msg.Id]));
                    return msg.Id;
                }
            }
            Intent intent;
            if (_waitingList.Contains(msg.Id))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "FILE ALREADY DOWNLOADING: " +msg.Id);
#endif
                intent = new Intent(_context, typeof(AttachmentsDownloadService));
                intent.SetAction(AttachmentsDownloadService.ActionStatusTag);
                _context.StartService(intent);
                StartLoadingEvent?.Invoke(this, new AttachmentHelperEventArgs<string>(msg.Id, msg.AttachUrl.Split('/').Last().ToLower(), msg.FromNumber));
                return msg.Id;
            }
            _waitingList.Add(msg.Id);
            _faxNotification.ShowNotification(DataFormatUtils.ToPhoneNumber(msg.FromNumber));
            intent = new Intent(_context, typeof(AttachmentsDownloadService));
            intent.SetAction(AttachmentsDownloadService.ActionStartTag);
            intent.PutExtra(AttachmentsDownloadService.ActionIdTag, msg.Id);
            intent.PutExtra(AttachmentsDownloadService.ActionMsgTag, msg);
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER SERVICE LAUNCHED: request ID=" + msg.Id);
#endif
            _context.StartService(intent);
            return msg.Id;
        }

        private void AttachmentsServiceOnStartEvent(object sender, AttachmentHelperEventArgs<string> args)
        {
            //string result;
            //var res = ContactsHelper.Instance(_context).GetName(args.Result, out result);
#if DEBUG
            Log.Debug(App.AppPackage, $"START LOADING ATTACHMENT FROM: {args.Result}");
#endif
            _faxNotification.ShowNotification(DataFormatUtils.ToPhoneNumber(args.Result));
            StartLoadingEvent?.Invoke(this, args);
        }

        private void AttachmentsServiceOnFailEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (args.Result)
                _faxNotification.HideNotification();
            else
                _faxNotification.FailLoading();
            _waitingList.Remove(args.Id);
            FailLoadingEvent?.Invoke(this, args);
        }

        private void AttachmentsServiceOnSuccessEvent(object sender, AttachmentHelperEventArgs<string> success)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "LOADING FINISHED: file path is " + success.Result);
#endif
            _cacheDictionary.Add(success.Id, success.Result);
            _waitingList.Remove(success.Id);
            if ((OnFinish == null) || (OnFinish.GetInvocationList().Length == 0))
            {
                var intent = new Intent(Intent.ActionView);
                var file = new Java.IO.File(success.Result);
                file.SetReadable(true);
                intent.SetDataAndType(Uri.FromFile(file), "application/pdf");
                intent.SetFlags(ActivityFlags.NoHistory);
                _faxNotification.FinishLoading(intent);
            }
            else
            {
                _faxNotification.HideNotification();
                OnFinish.Invoke(this, success);
            }
        }

        /// <summary>
        /// Receive result from service
        /// </summary>
        /// <param name="resultCode"></param>
        /// <param name="resultData"></param>
        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            
        }
    }
}