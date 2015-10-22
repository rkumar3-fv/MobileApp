using Android.Content;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public sealed class AppPreferencesHelper
    {
        private static volatile AppPreferencesHelper _instance;
        private static readonly object Locker = new object();
        private readonly ISharedPreferences _preferences;

        private const string AppPreferencesFile = "waprefs";
        private const string KeyIsFirstRun = "IsFirstRun";
        private const string KeyPhoneNumber = "PhoneNumber";

        private AppPreferencesHelper(Context context)
        {
            _preferences = context.GetSharedPreferences(AppPreferencesFile, FileCreationMode.Private);
        }

        /// <summary>
        /// Get application preferences helper instance
        /// </summary>
        public static AppPreferencesHelper Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (Locker)
            {
                if (_instance == null)
                _instance = new AppPreferencesHelper(context);
            }
            return _instance;
        }

        /// <summary>
        /// Check is app running first time
        /// </summary>
        public bool IsFirstRun()
        {
            return _preferences.GetBoolean(KeyIsFirstRun, true);
        }

        /// <summary>
        /// Get saved phone number
        /// </summary>
        /// <returns></returns>
        public string GetPhoneNumber()
        {
            return _preferences.GetString(KeyPhoneNumber, "");
        }

        /// <summary>
        /// Set not is first run flag
        /// </summary>
        public void SetNotIsFirstRun()
        {
            var editor = _preferences.Edit();
            editor.PutBoolean(KeyIsFirstRun, false);
            editor.Apply();
        }

        /// <summary>
        /// Save phone number to settings
        /// </summary>
        public void SavePhoneNumber(string phone)
        {
            var editor = _preferences.Edit();
            editor.PutString(KeyPhoneNumber, phone);
            editor.Apply();
        }
    }
}