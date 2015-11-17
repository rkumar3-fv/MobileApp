using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Android.Content;
using Android.Provider;
using Android.Support.V4.Util;
using com.FreedomVoice.MobileApp.Android.Utils;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    public sealed class AppPreferencesHelper
    {
        private static volatile AppPreferencesHelper _instance;
        private static readonly object Locker = new object();
        private readonly ISharedPreferences _preferences;
        private readonly Context _context;
        private readonly string _cookiePath;
        private const string AppPreferencesFile = "fvprefs";
        private const string KeyIsFirstRun = "IsFirstRun";
        private const string KeyPhoneNumber = "PhoneNumber";
        private const string KeyLogin = "Email";
        private const string KeyToken = "Token";
        private const string KeyAccount = "Acc";
        private const string KeyCallerId = "Caller";

        private AppPreferencesHelper(Context context)
        {
            _context = context;
            _preferences = context.GetSharedPreferences(AppPreferencesFile, FileCreationMode.Private);
            _cookiePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "cookie.dat");
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
        public string GetPhoneNumber()
        {
            return _preferences.GetString(KeyPhoneNumber, "");
        }

        /// <summary>
        /// Get credentials pair
        /// </summary>
        /// <param name="salt">salt</param>
        public Pair GetLoginPass(string salt)
        {
            var androidId = Settings.Secure.GetString(_context.ContentResolver, Settings.Secure.AndroidId);
            var loginEncrypted = _preferences.GetString(KeyLogin, "");
            var passwordEncrypted = _preferences.GetString(KeyToken, "");
            if ((loginEncrypted.Length == 0)||(passwordEncrypted.Length == 0))
                return new Pair("","");
            var login = SecurityHelper.Decrypt(loginEncrypted, androidId, salt);
            var pass = SecurityHelper.Decrypt(passwordEncrypted, salt, androidId);
            return new Pair(login, pass);
        }

        /// <summary>
        /// Get account data pair
        /// </summary>
        public Pair GetAccCaller()
        {
            var account = _preferences.GetString(KeyAccount, "");
            var caller = _preferences.GetString(KeyCallerId, "");
            return new Pair(account, caller);
        }

        /// <summary>
        /// Get cookie
        /// </summary>
        public CookieContainer GetCookieContainer()
        {
            return CookieSerializer.ReadCookiesFromDisk(_cookiePath);
        }

        public void ClearCredentials()
        {
            CookieSerializer.ClearCookies(_cookiePath);
            var editor = _preferences.Edit();
            editor.PutString(KeyLogin, "");
            editor.PutString(KeyToken, "");
            editor.Apply();
        }

        /// <summary>
        /// Set not is first run flag
        /// </summary>
        public void SetNotIsFirstRun()
        {
            Task.Factory.StartNew(() =>
            {
                var editor = _preferences.Edit();
                editor.PutBoolean(KeyIsFirstRun, false);
                editor.Apply();
            });
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

        /// <summary>
        /// Save last account data
        /// </summary>
        /// <param name="account">selected account</param>
        /// <param name="caller">selected caller</param>
        public void SaveAccCaller(string account, string caller)
        {
            var editor = _preferences.Edit();
            editor.PutString(KeyAccount, account);
            editor.PutString(KeyCallerId, caller);
            editor.Apply();
        }

        /// <summary>
        /// Save login and password
        /// </summary>
        /// <param name="login">user login</param>
        /// <param name="token">user password or token</param>
        /// <param name="salt">salt</param>
        public void SaveCredentials(string login, string token, string salt)
        {
            var androidId = Settings.Secure.GetString(_context.ContentResolver, Settings.Secure.AndroidId);
            Task.Factory.StartNew(() =>
            {
                var log = SecurityHelper.Encrypt(login, androidId, salt);
                var pass = SecurityHelper.Encrypt(token, salt, androidId);
                var editor = _preferences.Edit();
                editor.PutString(KeyLogin, log);
                editor.PutString(KeyToken, pass);
                editor.Apply();
            });
        }

        /// <summary>
        /// Save cookie container into internal storage
        /// </summary>
        /// <param name="cookie">used cookie container</param>
        public void SaveCookie(CookieContainer cookie)
        {
            Task.Factory.StartNew(() => { CookieSerializer.WriteCookiesToDisk(_cookiePath, cookie);});
        }
    }
}