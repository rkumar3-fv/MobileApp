using System;
using System.Net;
using Android.App;
using Android.Content;
using Android.Gms.Analytics;
using Android.Runtime;
using Android.Telephony;
using com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs;
#if DEBUG
using Android.Util;
#endif
#if TRACE
#if !DEBUG
using System.Threading.Tasks;
#endif
#endif
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
        private const string Analytics = "UA-69040520-1";
#if TRACE
#if !DEBUG
        public const string HockeyAppKey = "4f540a867b134c62b99fba824046466c";
#endif
#endif

        private Tracker _tracker;
        private AppHelper _helper;

        /// <summary>
        /// Analytics tracker
        /// </summary>
        public Tracker AnalyticsTracker
        {
            get
            {
                if (_tracker != null) return _tracker;
                var analytics = GoogleAnalytics.GetInstance(this);
                _tracker = analytics.NewTracker(Analytics);
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

#if TRACE
#if !DEBUG
            HockeyApp.CrashManager.Register(this, HockeyAppKey);
            HockeyApp.TraceWriter.Initialize();
            AndroidEnvironment.UnhandledExceptionRaiser += (sender, args) =>
            {
                HockeyApp.TraceWriter.WriteTrace(args.Exception);
                args.Handled = true;
            };
            AppDomain.CurrentDomain.UnhandledException +=
                (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.ExceptionObject);
            TaskScheduler.UnobservedTaskException +=
                (sender, args) => HockeyApp.TraceWriter.WriteTrace(args.Exception);
#endif
#endif

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
    }
}