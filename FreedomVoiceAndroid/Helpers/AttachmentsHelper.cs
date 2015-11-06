using System;
using System.Collections.Generic;
using System.IO;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Notifications;
using com.FreedomVoice.MobileApp.Android.Services;
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
    public class AttachmentsHelper : Java.Lang.Object, IServiceConnection
    {
        private readonly Context _context;
        private bool _isBound;
        private FaxForegroundService _faxService;
        private readonly Dictionary<int, string> _cacheDictionary;
        private readonly List<int> _waitingList;
        private readonly FaxUploadNotification _faxNotification;

        public event SuccessEventHandler OnFinish;
        public event StopLoadingEventHandler FailLoadingEvent;

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
            if (!_isBound)
                _context.BindService(new Intent(_context, typeof(FaxForegroundService)), this, Bind.AutoCreate);
            _context.StartService(intent);
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
            _faxService.SuccessEvent += FaxServiceOnSuccessEvent;
            _faxService.StartEvent += FaxServiceOnStartEvent;
            _faxService.FailEvent += FaxServiceOnFailEvent;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE UNBINDED");
#endif
            _faxService.SuccessEvent -= FaxServiceOnSuccessEvent;
            _faxService.StartEvent -= FaxServiceOnStartEvent;
            _isBound = false;
        }

        private void FaxServiceOnStartEvent(object sender, AttachmentHelperEventArgs<string> args)
        {
            string result;
            //var res = ContactsHelper.Instance(_context).GetName(args.Result, out result);
#if DEBUG
            Log.Debug(App.AppPackage, $"START LOADING ATTACHMENT FROM: {args.Result}");
#endif
            _faxNotification.ShowNotification(args.Result);
        }

        private void FaxServiceOnFailEvent(object sender, AttachmentHelperEventArgs<bool> args)
        {
            if (args.Result)
                _faxNotification.HideNotification();
            else
                _faxNotification.FailLoading();
            _waitingList.Remove(args.Id);
            FailLoadingEvent?.Invoke(this, args);
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