using Android;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Telephony;
using com.FreedomVoice.MobileApp.Android.Storage;
using Firebase.Analytics;
using Environment = Android.OS.Environment;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public enum TimingEvent
    {
        Request,
        FileLoading,
        LongAction
    }

    public enum SpecialEvent
    {
        LowMemory
    }

    /// <summary>
    /// Main application helper
    /// </summary>
    public class AppHelper
    {
        private readonly Context _context;

        public ReportHelper Reports { get; set; }

        /// <summary>
        /// App network actions helper
        /// </summary>
        public ActionsHelper ActionsHelper { get; set; }

        /// <summary>
        /// App attachments helper
        /// </summary>
        public AttachmentsHelper AttachmentsHelper { get; }

        /// <summary>
        /// App preferences helper
        /// </summary>
        public AppPreferencesHelper PreferencesHelper { get; }
        
        public NavigationRedirectHelper NavigationRedirectHelper { get; } = new NavigationRedirectHelper();

#region Permissions
        public const string MakeCallsPermission = Manifest.Permission.CallPhone;
        public const string ReadContactsPermission = Manifest.Permission.ReadContacts;
        public const string WriteStoragePermission = Manifest.Permission.WriteExternalStorage;
        public const string ReadStoragePermission = Manifest.Permission.ReadExternalStorage;
        public const string WakeLockStatePermission = Manifest.Permission.WakeLock;
        public const string AccessNetworkStatePermission = Manifest.Permission.AccessNetworkState;
        public const string InternetPermission = Manifest.Permission.Internet;
        public const string ReadPhoneStatePermission = Manifest.Permission.ReadPhoneState;

        /// <summary>
        /// Check CALL_PHONE permission
        /// </summary>
        public bool CheckReadPhoneState()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return _context.CheckSelfPermission(ReadPhoneStatePermission) == Permission.Granted;
        }

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
        public bool CheckFilesPermissions()
        {
            if ((int)Build.VERSION.SdkInt < 23)
                return true;
            return (_context.CheckSelfPermission(ReadStoragePermission) == Permission.Granted) &&
                   (_context.CheckSelfPermission(WriteStoragePermission) == Permission.Granted);
        }
#endregion

#region Analytics

        public const string InsightsKey = "d8bf081e25b6c01b8a852a950d1f7b446d33662d";

        /// <summary>
        /// Check Google FirebaseTracker state
        /// </summary>
        public FirebaseAnalytics FirebaseTracker { get; private set; }
        private const string RequestKey = "API_REQUEST";
        private const string LoadingKey = "LOADING_FILE";
        private const string ActionKey = "LONG_ACTION";
        private const string OtherKey = "OTHER";
        private const string LowMemoryKey = "LOW_MEMORY";

        public void InitFirebaseAnalytics()
        {
            FirebaseTracker = FirebaseAnalytics.GetInstance(_context);
        }
        
        /// <summary>
        /// Time reporting via Firebase
        /// </summary>
        /// <param name="type">action type</param>
        /// <param name="name">action name</param>
        /// <param name="result">action result</param>
        /// <param name="time">action duration</param>
        public bool ReportTime(TimingEvent type, string name, string result, long time)
        {
            if (FirebaseTracker == null) return false;
            var bundle = new Bundle();
            
            switch (type)
            {
                case TimingEvent.Request:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, RequestKey);
                    break;
                case TimingEvent.FileLoading:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, LoadingKey);
                    break;
                case TimingEvent.LongAction:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, ActionKey);
                    break;
                default:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, OtherKey);
                    break;
            }
            bundle.PutString(FirebaseAnalytics.Param.ItemName, name);
            bundle.PutString(FirebaseAnalytics.Param.Value, result);
            bundle.PutLong(FirebaseAnalytics.Param.StartDate, time);
            FirebaseTracker.LogEvent("report_time", bundle);
            return true;
        }

        public bool ReportEvent(SpecialEvent type, string name, string result)
        {
            if (FirebaseTracker == null) return false;
            var bundle = new Bundle();
            switch (type)
            {
                case SpecialEvent.LowMemory:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, LoadingKey);
                    break;
                default:
                    bundle.PutString(FirebaseAnalytics.Param.ItemCategory, OtherKey);
                    break;
            }
            bundle.PutString(FirebaseAnalytics.Param.ItemName, name);
            bundle.PutString(FirebaseAnalytics.Param.Value, result);
            FirebaseTracker.LogEvent("special_event", bundle);
            return true;
        }

        #endregion

#region Device State
        private string _phoneNumber;

        /// <summary>
        /// Check is airplane mode ON
        /// </summary>
        public bool IsAirplaneModeOn()
        {
            return (int) Build.VERSION.SdkInt < 17 ? IsAirplaneOldApi() : IsAirplaneNewApi();
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
            if (!CheckFilesPermissions()) return false;
            var state = Environment.ExternalStorageState;
            return state == Environment.MediaMounted;
        }

        /// <summary>
        /// Check phone type
        /// </summary>
        /// <returns></returns>
        public bool IsVoicecallsSupported()
        {
            if (!CheckCallsPermission()) return false;
            var tm = _context.GetSystemService(Context.TelephonyService).JavaCast<TelephonyManager>();
            if (tm != null)
                return tm.PhoneType != PhoneType.None;
            return false;
        }

        /// <summary>
        /// Get my phone number
        /// </summary>
        /// <returns>Phone number or null if not supported</returns>
        public string GetMyPhoneNumber()
        {
            if (!IsVoicecallsSupported())
            {
                _phoneNumber = null;
                PreferencesHelper.SavePhoneNumber("");
                return null;
            }
            _phoneNumber = PreferencesHelper.GetPhoneNumber();
            var telephony = _context.GetSystemService(Context.TelephonyService)?.JavaCast<TelephonyManager>();
            if (!CheckReadPhoneState())
            {
                if ((telephony?.Line1Number == null) || (telephony.SimSerialNumber == null) ||
                    (telephony.SimSerialNumber.Length == 0))
                {
                    _phoneNumber = null;
                    PreferencesHelper.SavePhoneNumber("");
                    return null;
                }
            }
            if (telephony?.Line1Number != null && CheckReadPhoneState() && (_phoneNumber != telephony.Line1Number) && (telephony.Line1Number.Length > 1) && (_phoneNumber.Length != 10))
            {
                if (telephony.Line1Number.StartsWith("1") && (telephony.Line1Number.Length == 11))
                    _phoneNumber = telephony.Line1Number.Substring(1);
                else if (telephony.Line1Number.StartsWith("+1") && (telephony.Line1Number.Length == 12))
                    _phoneNumber = telephony.Line1Number.Substring(2);
                else
                    _phoneNumber = telephony.Line1Number;
                PreferencesHelper.SavePhoneNumber(_phoneNumber);
                return _phoneNumber;
            }
            return _phoneNumber;
        }

        /// <summary>
        /// Set my phone number if voicecalls supported
        /// </summary>
        public bool SetMyPhoneNumber(string phoneNumber)
        {
            if (!IsVoicecallsSupported()) return false;
            if ((_phoneNumber != null) && (_phoneNumber == phoneNumber)) return true;
            _phoneNumber = phoneNumber;
            PreferencesHelper.SavePhoneNumber(phoneNumber);
            return true;
        }

#endregion

        public AppHelper(Context context)
        {
            _context = context;
            AttachmentsHelper = new AttachmentsHelper(context);
            PreferencesHelper = AppPreferencesHelper.Instance(_context);
        }
    }
}