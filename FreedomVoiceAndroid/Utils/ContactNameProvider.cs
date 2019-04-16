using System;
using Android.Content;
using FreedomVoice.Core.ViewModels;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class ContactNameProvider: IContactNameProvider
    {
        private readonly Context _context;

        public ContactNameProvider(Context context)
        {
            _context = context;
        }

        public event EventHandler ContactsUpdated;
        public string GetName(string phone)
        {
            var res = phone;
            ContactsHelper.Instance(_context).GetName(phone, out res);
            return res;
        }

        public void RequestContacts()
        {
            
        }

        
    }
}