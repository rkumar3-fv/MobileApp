using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.Net;
using Android.Provider;
using FreedomVoice.Core.Utils;
using Java.Interop;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    /// <summary>
    /// Contacts helper cache
    /// </summary>
    public class ContactsHelper
    {
        private static volatile ContactsHelper _instance;
        private static readonly object Locker = new object();
        private readonly Context _context;
        private readonly Dictionary<string, string> _phonesCache; 

        private ContactsHelper(Context context)
        {
            _context = context;
            _phonesCache = new Dictionary<string, string>();
        }

        /// <summary>
        /// Get contacts cache helper instance
        /// </summary>
        public static ContactsHelper Instance(Context context)
        {
            if (_instance != null) return _instance;
            lock (Locker)
            {
                if (_instance == null)
                    _instance = new ContactsHelper(context);
            }
            return _instance;
        }

        /// <summary>
        /// Get contact name by phone
        /// </summary>
        /// <param name="normalizedPhone">normalized phone</param>
        /// <returns>Is in contacts</returns>
        public bool GetName(string normalizedPhone, out string name)
        {
            if (_phonesCache.ContainsKey(normalizedPhone))
            {
                name = _phonesCache[normalizedPhone];
                return true;
            }

            var uri = Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, Uri.Encode(normalizedPhone));
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
            var loader = new CursorLoader(_context, uri, projection, null, null, null);
            var cursor = loader.LoadInBackground().JavaCast<ICursor>();
            
            if (cursor == null)
            {
                name = DataFormatUtils.ToPhoneNumber(normalizedPhone);
                return false;
            }
            if (cursor.MoveToFirst())
            {
                name = cursor.GetString(0);
                AddToCache(normalizedPhone, name);
                cursor.Close();
                return true;
            }
            name = DataFormatUtils.ToPhoneNumber(normalizedPhone);
            cursor.Close();
            return false;
        }

        private void AddToCache(string phone, string name)
        {
            _phonesCache.Add(phone, name);
        }
    }
}