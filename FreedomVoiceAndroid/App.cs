using System;
using System.Collections.Generic;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Helpers;
using Xamarin;
using Process = System.Diagnostics.Process;

namespace com.FreedomVoice.MobileApp.Android
{
    /// <summary>
    /// Reloaded entry point
    /// </summary>
    [Application
        (Label = "@string/ApplicationName",
        AllowBackup = false,
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/AppTheme")]
    public class App : Application
    {
        public const string AppPackage = "com.FreedomVoice.MobileApp.Android";
        private AppHelper _helper;

        /// <summary>
        /// Main application helper
        /// </summary>
        public AppHelper ApplicationHelper => _helper;

        protected App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {}

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

        public override void OnCreate()
        {
            base.OnCreate();
            _helper = new AppHelper(this);
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
#if !TRACE
            if (!_helper.IsInsigthsOn)
                AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
#endif
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

        protected void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}