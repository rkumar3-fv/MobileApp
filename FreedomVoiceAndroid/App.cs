using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Telephony;
using com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using Xamarin;

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
        private AppHelper _helper;

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
            _helper = AppHelper.Instance(this);
#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) =>
            {
                Log.Debug(AppPackage, sslPolicyErrors.ToString());
                return true;
            };
#else
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
#endif
#if TRACE
#if !DEBUG
            _helper.InitHockeyApp();
#endif
#endif
            CallState = new CallStateHelper();
            CallState.CallEvent += CallStateOnCallEvent;
            var telManager = (TelephonyManager)GetSystemService(TelephonyService);
            telManager.Listen(CallState, PhoneStateListenerFlags.CallState);
        }

        /// <summary>
        /// Outgoing call finished event
        /// </summary>
        private void CallStateOnCallEvent(object sender, DialingEventArgs args)
        {
            _helper.ActionsHelper.MarkCallAsFinished();
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            if (_helper.IsInsigthsOn)
            {
                var mi = new ActivityManager.MemoryInfo();
                var activityManager = GetSystemService(ActivityService).JavaCast<ActivityManager>();
                activityManager.GetMemoryInfo(mi);
                var availableMegs = mi.AvailMem/1048576L;
                var totalMegs = (int) Build.VERSION.SdkInt > 15 ? $"{mi.TotalMem}" : "API 15";
                var val = $"{DateTime.Now}: available {totalMegs}Mb / {availableMegs}Mb";
                var dict = new Dictionary<string, string> {{"LOW MEMORY", val}};
                Insights.Report(null, dict, Insights.Severity.Critical);
            }
        }
    }
}