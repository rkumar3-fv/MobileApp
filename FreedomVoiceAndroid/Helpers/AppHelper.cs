using System;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Analytics;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Utils;
using HockeyApp;
using Java.Util;
using Xamarin;
using Environment = Android.OS.Environment;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// Main application helper
    /// </summary>
    public class AppHelper
    {
#region Singleton
        private static volatile AppHelper _instance;
        private static readonly object Locker = new object();
        private readonly Context _context;

        /// <summary>
        /// Get application helper instance
        /// </summary>
        public static AppHelper Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (Locker)
            {
                if (_instance == null)
                    _instance = new AppHelper(context);
            }
            return _instance;
        }

        /// <summary>
        /// App network actions helper
        /// </summary>
        public ActionsHelper ActionsHelper { get; }

        /// <summary>
        /// App attachments helper
        /// </summary>
        public AttachmentsHelper AttachmentsHelper { get; }
#endregion

#region Permissions
        private const string MakeCallsPermission = Manifest.Permission.CallPhone;
        private const string ReadContactsPermission = Manifest.Permission.ReadContacts;
        private const string WriteStoragePermission = Manifest.Permission.WriteExternalStorage;
        private const string ReadStoragePermission = Manifest.Permission.ReadExternalStorage;
        private const string WakeLockStatePermission = Manifest.Permission.WakeLock;
        private const string AccessNetworkStatePermission = Manifest.Permission.AccessNetworkState;
        private const string InternetPermission = Manifest.Permission.Internet;

        /// <summary>
        /// Check WAKE_LOCK permission
        /// </summary>
        private bool CheckWakeLockPermission()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return _context.CheckSelfPermission(WakeLockStatePermission) == Permission.Granted;
        }

        /// <summary>
        /// Check INTERNET & ACCESS_NETWORK_STATE permission
        /// </summary>
        private bool CheckInternetPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return (_context.CheckSelfPermission(InternetPermission) == Permission.Granted) &&
                   (_context.CheckSelfPermission(AccessNetworkStatePermission) == Permission.Granted);
        }

        /// <summary>
        /// Check CALL_PHONE permission
        /// </summary>
        public bool CheckCallsPermission()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return _context.CheckSelfPermission(MakeCallsPermission) == Permission.Granted;
        }

        /// <summary>
        /// Check READ_CONTACTS permission
        /// </summary>
        public bool CheckContactsPermission()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return _context.CheckSelfPermission(ReadContactsPermission) == Permission.Granted;
        }

        /// <summary>
        /// Check WRITE_EXTERNAL_STORAGE & READ_EXTERNAL_STORAGE permissions
        /// </summary>
        public bool CheckFilesPremissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return (_context.CheckSelfPermission(ReadStoragePermission) == Permission.Granted) &&
                   (_context.CheckSelfPermission(WriteStoragePermission) == Permission.Granted);
        }
#endregion

#region Analytics
        private const string GaFreedomVoiceKey = "UA-587407-95";
        private const string GaWaveAccessKey = "UA-69040520-1";
        private const string InsightsWaveAccessKey = "96308ef2e65dff5994132a9a8b18021948dadc54";
        private const string HockeyAppWaveAccessKey = "4f540a867b134c62b99fba824046466c";

        private bool _isInsightsOn;

        /// <summary>
        /// Check Google Analytics state
        /// </summary>
        public bool IsGoogleAnalyticsOn { get; private set; }

        /// <summary>
        /// Check Xamarin Insights state
        /// </summary>
        public bool IsInsigthsOn => (_isInsightsOn && CheckFilesPremissions());

        /// <summary>
        /// Check HockeyApp state
        /// </summary>
        public bool IsHockeyAppOn { get; private set; }

        /// <summary>
        /// Get GA tracker or NULL if not active
        /// </summary>
        public Tracker AnalyticsTracker { get; private set; }

        /// <summary>
        /// Initialize GA tracking
        /// </summary>
        /// <param name="isReleaseVersion">Is Release version of App</param>
        /// <returns>GA state</returns>
        public bool InitGa(bool isReleaseVersion)
        {
            if (IsGoogleAnalyticsOn) return true;
            if (CheckInternetPermissions() && CheckWakeLockPermission())
            {
                var analytics = GoogleAnalytics.GetInstance(_context);
                AnalyticsTracker = analytics.NewTracker(isReleaseVersion?GaFreedomVoiceKey:GaWaveAccessKey);
                AnalyticsTracker.EnableAutoActivityTracking(true);
                AnalyticsTracker.EnableExceptionReporting(true);
                analytics.EnableAutoActivityReports(App.GetApplication(_context));
                var pInfo = _context.PackageManager.GetPackageInfo(App.AppPackage, 0);
                AnalyticsTracker.SetAppName(_context.GetString(Resource.String.ApplicationName));
                AnalyticsTracker.SetAppVersion($"{pInfo.VersionCode} ({pInfo.VersionName})");
                AnalyticsTracker.SetLanguage(Locale.Default.Language);
                AnalyticsTracker.SetScreenResolution(_context.Resources.DisplayMetrics.WidthPixels, _context.Resources.DisplayMetrics.HeightPixels);
                IsGoogleAnalyticsOn = true;
                return true;
            }
            IsGoogleAnalyticsOn = false;
            return false;
        }

        /// <summary>
        /// Initialize Xamarin Insights tracking
        /// </summary>
        /// <returns>Xamarin Insights state</returns>
        public bool InitInsights()
        {
            if (IsInsigthsOn) return true;
            if (!CheckInternetPermissions() || !CheckFilesPremissions())
            {
                _isInsightsOn = false;
                return false;
            }
            Insights.HasPendingCrashReport += (sender, isStartupCrash) =>
            {
                if (isStartupCrash)
                    Insights.PurgePendingCrashReports().Wait();
            };
            Insights.Initialize(InsightsWaveAccessKey, _context);
            _isInsightsOn = true;
            return true;
        }

        /// <summary>
        /// Initialize HockeyApp tracking & updater
        /// </summary>
        /// <returns>HockeyApp tracker state</returns>
        public bool InitHockeyApp()
        {
            if (IsHockeyAppOn) return true;
            if (CheckInternetPermissions() && CheckWakeLockPermission())
            {
                CrashManager.Register(_context, HockeyAppWaveAccessKey);
                TraceWriter.Initialize();
                AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
                {
                    TraceWriter.WriteTrace(args.Exception);
                    args.Handled = true;
                };
                AppDomain.CurrentDomain.UnhandledException +=
                    (sender, args) => TraceWriter.WriteTrace(args.ExceptionObject);
                TaskScheduler.UnobservedTaskException +=
                    (sender, args) => TraceWriter.WriteTrace(args.Exception);
                ExceptionSupport.UncaughtTaskExceptionHandler = TraceWriter.WriteTrace;
                IsHockeyAppOn = true;
                return true;
            }
            IsHockeyAppOn = false;
            return false;
        }

        public bool InitHockeyUpdater(Activity activity)
        {
            if (!IsHockeyAppOn) return false;
            if (!CheckFilesPremissions()) return false;
            UpdateManager.Register(activity, HockeyAppWaveAccessKey);
            return true;
        }
#endregion

#region Device State
        /// <summary>
        /// Check is airplane mode ON
        /// </summary>
        public bool IsAirplaneModeOn()
        {
            return (int)Build.VERSION.SdkInt < 17 ? IsAirplaneOldApi() : IsAirplaneNewApi();
        }

        //@SuppressLint("NewApi")
        private bool IsAirplaneNewApi()
        {
            return Settings.Global.GetInt(_context.ContentResolver, Settings.Global.AirplaneModeOn, 0) != 0;
        }

        //@SuppressWarnings("deprecation")
        private bool IsAirplaneOldApi()
        {
#pragma warning disable 618
            return Settings.System.GetInt(_context.ContentResolver, Settings.System.AirplaneModeOn, 0) != 0;
#pragma warning restore 618
        }

        /// <summary>
        /// Check caller ID state
        /// <b>No API method available for getting caller ID state</b>
        /// </summary>
        public bool IsCallerIdHides()
        {
            return false;
        }

        /// <summary>
        /// Check is internet connected
        /// </summary>
        public bool IsInternetConnected()
        {
            if (!CheckInternetPermissions()) return false;
            var conMgr = _context.GetSystemService(Context.ConnectivityService).JavaCast<ConnectivityManager>();
            var info = conMgr?.ActiveNetworkInfo;
            var isAvailable = info?.IsAvailable ?? false;
            var isConnected = info?.IsConnected ?? false;
            return isAvailable && isConnected;
        }

        /// <summary>
        /// Check external storage
        /// </summary>
        public bool IsStorageAvailable()
        {
            if (!CheckFilesPremissions()) return false;
            var state = Environment.ExternalStorageState;
            return state == Environment.MediaMounted;
        }
#endregion

        private AppHelper(Context context)
        {
            _context = context;
            ActionsHelper = new ActionsHelper(App.GetApplication(context));
            AttachmentsHelper = new AttachmentsHelper(context);
        }
    }
}