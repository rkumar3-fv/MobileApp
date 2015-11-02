using System.Collections.Generic;
using System.IO;
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

    public delegate void ProgressEventHandler(object sender, AttachmentHelperEventArgs<int> args);

    public delegate void StartLoadingEventHandler(object sender, AttachmentHelperEventArgs<string> args);

    /// <summary>
    /// Attachments managment
    /// </summary>
    public class AttachmentsHelper : Java.Lang.Object, IServiceConnection
    {
        private readonly Context _context;
        private bool _isBound;
        private FaxForegroundService _faxService;
        private readonly Dictionary<int, string> _cacheDictionary;
        private readonly List<int> _waitingList;
        private readonly FaxUploadNotification _faxNotification;

        public event ProgressEventHandler OnProgress;
        public event SuccessEventHandler OnFinish; 

        public AttachmentsHelper(Context context)
        {
            _context = context;
            _cacheDictionary = new Dictionary<int, string>();
            _waitingList = new List<int>();
            _faxNotification = FaxUploadNotification.Instance(_context);
        }

        public long LoadFaxAttachment(Message msg)
        {
            if (_cacheDictionary.ContainsKey(msg.Id))
            {
                if (File.Exists(_cacheDictionary[msg.Id]))
                {
#if DEBUG
                    Log.Debug(App.AppPackage, "FILE ALREADY DOWNLOADED: " + _cacheDictionary[msg.Id]);
#endif
                    OnFinish?.Invoke(this, new AttachmentHelperEventArgs<string>(msg.Id, _cacheDictionary[msg.Id]));
                    return 0;
                }
            }
            if (_waitingList.Contains(msg.Id))
            {
#if DEBUG
                Log.Debug(App.AppPackage, "FILE ALREADY DOWNLOADING: " +msg.Id);
#endif
                return msg.Id;
            }
            _waitingList.Add(msg.Id);
            var intent = new Intent(_context, typeof(FaxForegroundService));
            intent.SetAction(FaxForegroundService.ActionExecuteTag);
            intent.PutExtra(FaxForegroundService.ActionIdTag, msg.Id);
            intent.PutExtra(FaxForegroundService.ActionNameTag, msg.FromNumber);
            intent.PutExtra(FaxForegroundService.ActionTag, msg.AttachUrl);
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER SERVICE LAUNCHED: request ID=" + msg.Id);
#endif
            _context.StartService(intent);
            if (!_isBound)
                _context.BindService(new Intent(_context, typeof (FaxForegroundService)), this, Bind.Important);
            return msg.Id;
        }

        public void OnServiceConnected(ComponentName name, IBinder service)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE BINDED");
#endif
            var serviceBinder = service as FaxForegroundBinder;
            if (serviceBinder == null) return;
            _faxService = serviceBinder.Service;
            _isBound = true;
            _faxService.ProgressEvent += FaxServiceOnProgressEvent;
            _faxService.SuccessEvent += FaxServiceOnSuccessEvent;
            _faxService.StartEvent += FaxServiceOnStartEvent;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE UNBINDED");
#endif
            _faxService.ProgressEvent -= FaxServiceOnProgressEvent;
            _faxService.SuccessEvent -= FaxServiceOnSuccessEvent;
            _faxService.StartEvent -= FaxServiceOnStartEvent;
            _isBound = false;
        }

        private void FaxServiceOnStartEvent(object sender, AttachmentHelperEventArgs<string> args)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "START LOADING ATTACHMENT FROM: " + args.Result);
#endif
            _faxNotification.ShowNotification(DataFormatUtils.ToPhoneNumber(args.Result));
        }

        private void FaxServiceOnProgressEvent(object sender, AttachmentHelperEventArgs<int> progress)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "LOADING PROGRESS: " + progress.Result);
#endif
            _faxNotification.UpdateProgress(progress.Result);
            OnProgress?.Invoke(this, new AttachmentHelperEventArgs<int>(progress.Id, progress.Result));
        }

        private void FaxServiceOnSuccessEvent(object sender, AttachmentHelperEventArgs<string> success)
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
                OnFinish.Invoke(this, new AttachmentHelperEventArgs<string>(success.Id, success.Result));
            }
        }
    }
}