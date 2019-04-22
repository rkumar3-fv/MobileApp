using System;
using Android.Content;
using FreedomVoice.Core.ViewModels;

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