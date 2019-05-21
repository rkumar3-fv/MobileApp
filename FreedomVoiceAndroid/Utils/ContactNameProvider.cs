using System;
using System.Text.RegularExpressions;
using Android.Content;
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
            return phoneNumber;
        }

        public string GetClearPhoneNumber(string formattedPhoneNumber)
        {
            const string pattern = @"\d"; 
        
            var sb = "";
            foreach (Match m in Regex.Matches(formattedPhoneNumber, pattern))
                sb += m;

            return sb;
        }

        public void RequestContacts()
        {
       
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            ContactsHelper.Instance(_context).ContactsPermissionUpdated -= ProviderOnContactsUpdated;
            ContactsUpdated?.Invoke(null, null);
        }

    }
}