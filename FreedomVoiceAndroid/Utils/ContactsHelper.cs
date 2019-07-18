using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Util;
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
        // normalized phone (only numbers) to contact name 
        private readonly Dictionary<string, string> _phonesCache = new Dictionary<string, string>();
        private readonly AppHelper _appHelper;
        private readonly IPhoneFormatter _phoneFormatter = ServiceContainer.Resolve<IPhoneFormatter>();

        private ContactsHelper(Context context)
        {
            _context = context;
            _appHelper = App.GetApplication(_context).ApplicationHelper;
            if (_appHelper.CheckContactsPermission())
            {
                StartListenContactsChanges();
            }
        }

        private void ContactsObserverOnContactsChangingEvent(object sender, bool b)
        {
           ReloadContactsCache();
           ContactsPermissionUpdated?.Invoke(null, null);
        }

        private void ReloadContactsCache()
        {
            _phonesCache.Clear();

            string[] projection =
            {
                ContactsContract.Contacts.InterfaceConsts.Id,
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber
            };

            var selection = string.Format(
                "(({0} IS NOT NULL) AND ({0} != '') AND ({1} = '1'))",
                ContactsContract.Contacts.InterfaceConsts.DisplayName,
                ContactsContract.Contacts.InterfaceConsts.InVisibleGroup
            );
            var loader = new CursorLoader(_context,
                ContactsContract.Contacts.ContentUri,
                projection,
                selection,
                null,
                null
            );
            try
            {
                ICursor cursor = loader.LoadInBackground().JavaCast<ICursor>();
                var idIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.Id);
                var displayNameIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.DisplayName);
                var hasPhoneIndex = cursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber);

                if (cursor.MoveToFirst())
                {
                    do
                    {
                        var contactId = cursor.GetString(idIndex);
                        var name = cursor.GetString(displayNameIndex);
                        var hasPhone = cursor.GetInt(hasPhoneIndex);

                        if (hasPhone > 0)
                        {
                            var numberCursor = new CursorLoader(
                                _context,
                                ContactsContract.CommonDataKinds.Phone.ContentUri,
                                new[] {ContactsContract.CommonDataKinds.Phone.Number},
                                ContactsContract.CommonDataKinds.Phone.InterfaceConsts.ContactId + "=?",
                                new[] {contactId},
                                null
                            ).LoadInBackground().JavaCast<ICursor>();

                            if (numberCursor.MoveToFirst())
                            {
                                var numberIndex = numberCursor.GetColumnIndex(ContactsContract.CommonDataKinds.Phone.Number);
                                do
                                {
                                    var number = numberCursor.GetString(numberIndex);
                                    if (_phonesCache != null) _phonesCache[_phoneFormatter.Normalize(number)] = name;
                                } while (numberCursor.MoveToNext());
                            }
                            numberCursor.Close();
                        }
                    } while (cursor.MoveToNext());
                    cursor.Close();
                }
            }
            catch (JavaException e)
            {
                Log.Error(this.GetType().Name, e.StackTrace);
            }
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
            ReloadContactsCache();
            var contactsObserver = new ContactsObserver();
            _context.ContentResolver.RegisterContentObserver(ContactsContract.Contacts.ContentUri, true, contactsObserver);
            contactsObserver.ContactsChangingEvent += ContactsObserverOnContactsChangingEvent;
        }

        /// <summary>
        /// Get contact name by phone
        /// </summary>
        /// <param name="phone">normalized phone</param>
        /// <returns>Is in contacts</returns>
        public bool GetName(string phone, out string name)
        {
            var rawNumber = _phoneFormatter.Normalize(phone);
            var res = _GetName(rawNumber, out name);

            if (!res) {
                rawNumber = _phoneFormatter.NormalizeNational(phone);
                res = _GetName(rawNumber, out name);
            }
            return res;
        }

        private bool _GetName(string normalizedPhone, out string name)
        {
            if (_phonesCache.ContainsKey(normalizedPhone))
            {
                name = _phonesCache[normalizedPhone];
                return true;
            }

            name = normalizedPhone;
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