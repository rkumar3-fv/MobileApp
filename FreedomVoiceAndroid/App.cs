using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
#endif
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;
using Java.Lang;
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
            JavaSystem.SetProperty("http.keepAlive", "true");
            _helper = new AppHelper(this);
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
            if (!_helper.IsInsigthsOn)
                AppDomain.CurrentDomain.UnhandledException += HandleUnhandledException;
        }

        public override void OnLowMemory()
        {
            base.OnLowMemory();
            var mi = new ActivityManager.MemoryInfo();
            var activityManager = GetSystemService(ActivityService).JavaCast<ActivityManager>();
            activityManager.GetMemoryInfo(mi);
            var availableMegs = mi.AvailMem / 1048576L;
            var totalMegs = (int)Build.VERSION.SdkInt > 15 ? $"{mi.TotalMem / 1048576L}Mb" : "API 15";
#if DEBUG
            Log.Debug(AppPackage, $"LOW MEMORY: available  {availableMegs}Mb / {totalMegs}");
#endif
            ApplicationHelper.ReportEvent(SpecialEvent.LowMemory, DataFormatUtils.ToFullFormattedDate(DateTime.Now), $"{availableMegs}Mb / {totalMegs}");
        }

        protected void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }
    }
}