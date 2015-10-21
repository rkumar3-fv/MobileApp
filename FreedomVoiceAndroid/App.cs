using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Gms.Analytics;
using Android.Runtime;
using Android.Telephony;
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android
{
    /// <summary>
    /// Reloaded entry point
    /// </summary>
    [Application
        (Label = "@string/ApplicationName",
        AllowBackup = true,
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/AppTheme")]
    public class App : Application
    {
        public const string AppPackage = "com.FreedomVoice.MobileApp.Android";
        private Tracker _tracker;

        /// <summary>
        /// Analytics tracker
        /// </summary>
        public Tracker AnalyticsTracker
        {
            get
            {
                if (_tracker != null) return _tracker;
                var analytics = GoogleAnalytics.GetInstance(this);
                _tracker = analytics.NewTracker("UA-69040520-1");
                return _tracker;
            }
        }

        /// <summary>
        /// Call state helper
        /// </summary>
        public CallStateHelper CallState { get; private set; }

        /// <summary>
        /// Get app context
        /// </summary>
        public static App GetApplication(Context context)
        {
            var app = context as App;
            if (app != null)
                return app;
            return (App)context.ApplicationContext;
        }

        public App (IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        { }

        public override void OnCreate()
        {
            base.OnCreate();
            Helper = new ActionsHelper(this);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                Log.Debug(AppPackage, $"Certificate error: {sslPolicyErrors}");
                return true;
            };
            CallState = new CallStateHelper();
            CallState.CallEvent += CallStateOnCallEvent;
            var telManager = (TelephonyManager)GetSystemService(TelephonyService);
            telManager.Listen(CallState, PhoneStateListenerFlags.CallState);
        }

        private void CallStateOnCallEvent(object sender, DialingEventArgs args)
        {
            var intent = PackageManager.GetLaunchIntentForPackage(BaseContext.PackageName);
            intent.AddFlags(ActivityFlags.ClearTop);
            StartActivity(intent);
        }

        /// <summary>
        /// Application actions helper
        /// </summary>
        public ActionsHelper Helper { get; private set; }
    }
}