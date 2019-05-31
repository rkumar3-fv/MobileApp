using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Database;
using Android.Provider;
using com.FreedomVoice.MobileApp.Android.Activities;
using com.FreedomVoice.MobileApp.Android.Helpers;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
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
        public event EventHandler ContactsPermissionUpdated;
        private readonly Context _context;
        private readonly Dictionary<string, string> _phonesCache;
        private readonly AppHelper _appHelper;

        private ContactsHelper(Context context)
        {
            _context = context;
            _phonesCache = new Dictionary<string, string>();
            _appHelper = App.GetApplication(_context).ApplicationHelper;
            if (_appHelper.CheckContactsPermission())
                StartListenContactsChanges();
        }

        private void ContactsObserverOnContactsChangingEvent(object sender, bool b)
        {
           if ((_phonesCache != null)&&(_phonesCache.Count > 0))
                _phonesCache.Clear();

            ContactsPermissionUpdated?.Invoke(null, null);
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

        public void ContactsPermissionGranted() => StartListenContactsChanges();

        private void StartListenContactsChanges()
        {
            var contactsObserver = new ContactsObserver();
            _context.ContentResolver.RegisterContentObserver(ContactsContract.Contacts.ContentUri, true, contactsObserver);
            contactsObserver.ContactsChangingEvent += ContactsObserverOnContactsChangingEvent;
        }

    /// <summary>
    /// Get contact name by phone
    /// </summary>
    /// <param name="normalizedPhone">normalized phone</param>
    /// <returns>Is in contacts</returns>
    public bool GetName(string normalizedPhone, out string name)
        {
            var phone = ServiceContainer.Resolve<IPhoneFormatter>().Normalize(normalizedPhone);
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
                name = ServiceContainer.Resolve<IPhoneFormatter>().Format(phone);
                return false;
            }
            if (cursor.MoveToFirst())
            {
                name = cursor.GetString(0);
                AddToCache(phone, name);
                cursor.Close();
                return true;
            }
            name = ServiceContainer.Resolve<IPhoneFormatter>().Format(phone);
            cursor.Close();
            return false;
        }
        
        public ICursor Search(string enteredQuery)
        {
            var query = enteredQuery.Trim();
            var sortOrder = $"{ContactsContract.Contacts.InterfaceConsts.DisplayName} COLLATE LOCALIZED ASC";
            ICursor phonesCursor = null;
            var uri = ContactsContract.Contacts.ContentUri;
            string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
            var selection = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({0} like '%{2}%'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, query);
            var loader = new CursorLoader(_context, uri, projection, selection, null, sortOrder);
            ICursor namesCursor;
            try
            {
                namesCursor = loader.LoadInBackground().JavaCast<ICursor>();
            }
            catch (Java.Lang.RuntimeException)
            {
                namesCursor = null;
            }

            if (Regex.IsMatch(query, @"^[0-9+()\-\s]+$"))
            {
                var iDs = new List<string>();
                if ((namesCursor != null) && (namesCursor.Count > 0))
                {
                    while (namesCursor.MoveToNext())
                    {
                        var id = namesCursor.GetString(namesCursor.GetColumnIndex(projection[0]));
                        if (!string.IsNullOrEmpty(id))
                            iDs.Add(id);
                    }
                }

                var uriPhones = Uri.Parse($"content://com.android.contacts/data/phones/filter/*{ServiceContainer.Resolve<IPhoneFormatter>().Normalize(query)}*");
                string[] projectionPhones = { "contact_id", ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber, ContactsContract.Contacts.InterfaceConsts.PhotoUri };
                string selectionPhones;
                if (iDs.Count == 0)
                    selectionPhones = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                    ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup);
                else
                    selectionPhones = string.Format("(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1') AND ({2} NOT IN ('{3}')))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName, ContactsContract.Contacts.InterfaceConsts.InVisibleGroup, "contact_id", string.Join("', '", iDs.ToArray()));
                var loaderPhones = new CursorLoader(_context, uriPhones, projectionPhones, selectionPhones, null, sortOrder);
                try
                {
                    phonesCursor = loaderPhones.LoadInBackground().JavaCast<ICursor>();
                }
                catch (Java.Lang.RuntimeException)
                {
                    phonesCursor = null;
                }
            }

            if (phonesCursor == null)
                return namesCursor;
            if ((namesCursor == null)||(namesCursor.Count == 0))
                return phonesCursor;
            return new MergeCursor(new[] {phonesCursor, namesCursor});
        }


        private void AddToCache(string phone, string name)
        {
            _phonesCache.Add(phone, name);
        }
    }
}