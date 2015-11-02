using System;
using Android.Content;
using Android.OS;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Services;
using Java.Util.Concurrent.Atomic;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// Attachments managment
    /// </summary>
    public class AttachmentsHelper : Java.Lang.Object, IServiceConnection
    {
        private readonly Context _context;
        private readonly AtomicLong _idCounter;
        private bool _isBound;
        private FaxForegroundService _faxService;

        public event EventHandler<int> OnProgress;
        public event EventHandler<string> OnFinish; 

        public AttachmentsHelper(Context context)
        {
            _context = context;
            _idCounter = new AtomicLong();
        }

        public long LoadAttachment(string url)
        {
            var id = _idCounter.IncrementAndGet();
            var intent = new Intent(_context, typeof(FaxForegroundService));
            intent.SetAction(FaxForegroundService.ActionExecuteTag);
            intent.PutExtra(FaxForegroundService.ActionIdTag, id);
            intent.PutExtra(FaxForegroundService.ActionTag, url);
#if DEBUG
            Log.Debug(App.AppPackage, "HELPER SERVICE LAUNCHED: request ID=" + id);
#endif
            _context.StartService(intent);
            if (!_isBound)
                _context.BindService(new Intent(_context, typeof (FaxForegroundService)), this, Bind.Important);
            return id;
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
        }

        public void OnServiceDisconnected(ComponentName name)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "SERVICE UNBINDED");
#endif
            _faxService.ProgressEvent -= FaxServiceOnProgressEvent;
            _faxService.SuccessEvent -= FaxServiceOnSuccessEvent;
            _isBound = false;
        }

        private void FaxServiceOnProgressEvent(object sender, int i)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "LOADING PROGRESS: " + i);
#endif
            OnProgress?.Invoke(this, i);
        }

        private void FaxServiceOnSuccessEvent(object sender, string s)
        {
#if DEBUG
            Log.Debug(App.AppPackage, "LOADING FINISHED: file path is " + s);
#endif
            OnFinish?.Invoke(this, s);
        }
    }
}