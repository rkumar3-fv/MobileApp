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
using FreedomVoice.Core.ViewModels;
using Java.Lang;
using Microsoft.Extensions.Primitives;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class ContactNameProvider: IContactNameProvider
    {
        private readonly Context _context;
        public event EventHandler ContactsUpdated;

        public ContactNameProvider(Context context)
        {
            _context = context;
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
            const string pattern = @"\d"; 
        
            var sb = "";
            foreach (Match m in Regex.Matches(formattedPhoneNumber, pattern))
                sb += m;

            return sb;
        }

        public List<string> SearchNumbers(string query)
        {
            var res = new List<string>();
            if (string.IsNullOrEmpty(query))
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
                    var phone = DataFormatUtils.NormalizePhone(phoneCursor.GetString(phoneCursor.GetColumnIndex(projection[1])));
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

            Regex phoneParser;
            string format;

            switch( phoneNumber.Length ) {

                case 5 :
                    phoneParser = new Regex(@"(\d{3})(\d{2})");
                    format      = "$1 $2";
                    break;

                case 6 :
                    phoneParser = new Regex(@"(\d{2})(\d{2})(\d{2})");
                    format      = "$1 $2 $3";
                    break;

                case 7 :
                    phoneParser = new Regex(@"(\d{3})(\d{2})(\d{2})");
                    format      = "$1 $2 $3";
                    break;

                case 8 :
                    phoneParser = new Regex(@"(\d{4})(\d{2})(\d{2})");
                    format      = "$1 $2 $3";
                    break;

                case 9 :
                    phoneParser = new Regex(@"(\d{4})(\d{3})(\d{2})(\d{2})");
                    format      = "($1 $2 $3 $4";
                    break;

                case 10 :
                    phoneParser = new Regex(@"(\d{3})(\d{3})(\d{2})(\d{2})");
                    format      = "($1) $2-$3$4";
                    break;

                case 11 :
                    phoneParser = new Regex(@"(\d{4})(\d{3})(\d{2})(\d{2})");
                    format      = "$1 $2 $3 $4";
                    break;

                default:
                    return phoneNumber;
            }
            return phoneParser.Replace( phoneNumber, format );
        }

    }
}