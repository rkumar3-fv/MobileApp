using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Android.Content;
using Android.Database;
using Android.Provider;
using Android.Runtime;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.Core.ViewModels;
using Java.Lang;
using Microsoft.Extensions.Primitives;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class ContactNameProvider: IContactNameProvider
    {
        private readonly Context _context;
        private readonly IPhoneFormatter _formatter;
        public event EventHandler ContactsUpdated;

        public ContactNameProvider(Context context)
        {
            _context = context;
            _formatter = ServiceContainer.Resolve<IPhoneFormatter>();
            ContactsHelper.Instance(_context).ContactsPermissionUpdated += ProviderOnContactsUpdated;
        }


        public string GetName(string phone)
        {
            var res = phone;
            ContactsHelper.Instance(_context).GetName(phone, out res);
            return res;
        }

        public string GetNameOrNull(string phone)
        {
            return GetName(phone);
        }

        public string GetFormattedPhoneNumber(string phoneNumber)
        {
            return FormatPhoneNumber(phoneNumber);
        }

        public string GetClearPhoneNumber(string formattedPhoneNumber)
        {
            return _formatter.NormalizeNational(formattedPhoneNumber);
        }

        public List<string> SearchNumbers(string query)
        {
            var res = new List<string>();
            if (string.IsNullOrEmpty(query) || !App.GetApplication(_context).ApplicationHelper.CheckContactsPermission())
                return res;

            var contactCursor = ContactsHelper.Instance(_context).Search(query);

            while (contactCursor != null && contactCursor.MoveToNext())
            {
                var id = contactCursor.GetString(0);
                var hasPhone = contactCursor.GetString(contactCursor.GetColumnIndex(ContactsContract.Contacts.InterfaceConsts.HasPhoneNumber));
                if (hasPhone.Equals("0")) continue;
                string[] projection = { ContactsContract.Contacts.InterfaceConsts.Id, ContactsContract.CommonDataKinds.Phone.Number, "data2" };
                var loader = new CursorLoader(_context, ContactsContract.CommonDataKinds.Phone.ContentUri, projection,
                    $"contact_id={id}", null, null);
                ICursor phoneCursor;
                try
                {
                    phoneCursor = loader.LoadInBackground().JavaCast<ICursor>();
                }
                catch (RuntimeException)
                {
                    phoneCursor = null;
                }
                while (phoneCursor != null && phoneCursor.MoveToNext())
                {
                    var phone = ServiceContainer.Resolve<IPhoneFormatter>().NormalizeNational(phoneCursor.GetString(phoneCursor.GetColumnIndex(projection[1])));
                    res.Add(GetClearPhoneNumber(phone));
                }
                phoneCursor.Close();

            }

            return res;
        }

        public void RequestContacts()
        {
       
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            ContactsHelper.Instance(_context).ContactsPermissionUpdated -= ProviderOnContactsUpdated;
            ContactsUpdated?.Invoke(null, null);
        }
        
        private string FormatPhoneNumber(string phoneNumber) {

            if ( string.IsNullOrEmpty(phoneNumber) )
                return phoneNumber;

            return _formatter.Format(phoneNumber);
        }

    }
}