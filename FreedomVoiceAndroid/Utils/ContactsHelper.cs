using System.Collections.Generic;
using Android.Content;
using Android.Database;
using Android.Provider;
using FreedomVoice.Core.Utils;
using Java.Interop;
using Uri = Android.Net.Uri;

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
            var contactsObserver = new ContactsObserver();
            context.ContentResolver.RegisterContentObserver(ContactsContract.Contacts.ContentUri, true, contactsObserver);
            contactsObserver.ContactsChangingEvent += ContactsObserverOnContactsChangingEvent;
        }

        private void ContactsObserverOnContactsChangingEvent(object sender, bool b)
        {
           if ((_phonesCache != null)&&(_phonesCache.Count > 0))
                _phonesCache.Clear();
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
            var phone = DataFormatUtils.NormalizePhone(normalizedPhone);
            if (_phonesCache.ContainsKey(phone))
            {
                name = _phonesCache[phone];
                return true;
            }

            var uri = Uri.WithAppendedPath(ContactsContract.PhoneLookup.ContentFilterUri, Uri.Encode(phone));
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.DisplayName };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
            var loader = new CursorLoader(_context, uri, projection, selection, null, null);
            ICursor cursor;
            try
            {
                cursor = loader.LoadInBackground().JavaCast<ICursor>();
            }
            catch (Java.Lang.RuntimeException)
            {
                cursor = null;
            }
            
            
            if (cursor == null)
            {
                name = DataFormatUtils.ToPhoneNumber(phone);
                return false;
            }
            if (cursor.MoveToFirst())
            {
                name = cursor.GetString(0);
                AddToCache(phone, name);
                cursor.Close();
                return true;
            }
            name = DataFormatUtils.ToPhoneNumber(phone);
            cursor.Close();
            return false;
        }

        private void AddToCache(string phone, string name)
        {
            _phonesCache.Add(phone, name);
        }
    }
}