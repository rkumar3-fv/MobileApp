using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
#if DEBUG
using Android.Util;
using com.FreedomVoice.MobileApp.Android.Data;
#endif
using com.FreedomVoice.MobileApp.Android.Helpers;
using com.FreedomVoice.MobileApp.Android.Utils;
using Firebase;
using FreedomVoice.Core.Cache;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using Java.Lang;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
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
    public class App : Application, Application.IActivityLifecycleCallbacks
    {
        public const string AppPackage = "com.FreedomVoice.MobileApp";
        private readonly AppHelper _helper;
        private const string appCenterId = "ee6f6d34-7517-4926-9050-3e117c3406de";

        /// <summary>
        /// Main application helper
        /// </summary>
        public AppHelper ApplicationHelper => _helper;

        public App(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
            var preserveDateTimeMethods = DateTime.Now.AddYears(1).AddMonths(1).AddDays(1).AddHours(1).AddMinutes(1).AddSeconds(1);
            _helper = new AppHelper(this);
            ServiceContainer.Register<IContactNameProvider>(() => new ContactNameProvider(this));
        }

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
            AppCenter.Start(appCenterId,
                   typeof(Analytics), typeof(Crashes));
            FirebaseApp.InitializeApp(this);
            _helper.ActionsHelper = new ActionsHelper(this);
            RegisterActivityLifecycleCallbacks(this);
            JavaSystem.SetProperty("http.keepAlive", "true");
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
#else
            ApplicationHelper.Reports?.Log($"LOW MEMORY: available  {availableMegs}Mb / {totalMegs}");
#endif
            ApplicationHelper.ReportEvent(SpecialEvent.LowMemory, DataFormatUtils.ToFullFormattedDate(DateTime.Now), $"{availableMegs}Mb / {totalMegs}");
        }

        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Process.GetCurrentProcess().Kill();
        }


        public bool IsAppInForeground => _resumedActivitys > 0;
        public bool IsColdStart = true;
        private int _resumedActivitys = 0;
        
        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
        }

        public void OnActivityDestroyed(Activity activity)
        {
        }

        public void OnActivityPaused(Activity activity)
        {
            _resumedActivitys--;
        }

        public void OnActivityResumed(Activity activity)
        {
            _resumedActivitys++;
            IsColdStart = false;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {
        }

        public void OnActivityStarted(Activity activity)
        {
        }

        public void OnActivityStopped(Activity activity)
        {
        }
    }
}