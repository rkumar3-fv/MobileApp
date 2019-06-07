using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.Core.ViewModels;

namespace FreedomVoice.iOS.Core.Utilities.Helpers
{
    class ContactNameProvider: IContactNameProvider
    {

        private Dictionary<string, string> _contactNames = new Dictionary<string, string>();
        private readonly IPhoneFormatter _phoneFormatter = FreedomVoice.Core.Utils.ServiceContainer.Resolve<IPhoneFormatter>();

        public event EventHandler ContactsUpdated;
        
        public ContactNameProvider()
        {
            ContactItemsDidReceive(null, null);

            Contacts.ItemsChanged += ContactItemsDidReceive;
        }

        public List<string> SearchNumbers(string query)
        {
            var res = new List<string>();
            var contacts = Contacts.ContactList.Where(c => Contacts.ContactMatchPredicate(c, query)).Distinct().ToList();
            foreach (var contact in contacts)
            {
                res.AddRange(contact.Phones.Select(phone => GetClearPhoneNumber(phone.Number)));
            }
            return res;
        }

        public void RequestContacts()
        {
            Helpers.Contacts.GetContactsListAsync();
        }
        
        public string GetName(string phone)
        {
            return _contactNames.ContainsKey(phone) ? _contactNames[phone] : FormatPhoneNumber(phone);
        }

        public string GetNameOrNull(string phone)
        {
            return _contactNames.ContainsKey(phone) ? _contactNames[phone] : null;
        }

        public string GetFormattedPhoneNumber(string phoneNumber)
        {
            return FormatPhoneNumber(phoneNumber);
        }
        
        public string GetClearPhoneNumber(string formattedNumber)
        {
            return _phoneFormatter.Normalize(formattedNumber);
        }

        private void ContactItemsDidReceive(object sender, EventArgs e)
        {
            foreach(var contact in Contacts.ContactList)
            {
                foreach(var phone in contact.Phones) {
                    var raw = _phoneFormatter.Normalize(phone.Number);
                    _contactNames[raw] = contact.DisplayName;
                    var rawNational = _phoneFormatter.NormalizeNational(phone.Number);
                    if( !raw.Equals(rawNational) )
                    {
                        _contactNames[rawNational] = contact.DisplayName;
                    }
                }
            }
            Contacts.ItemsChanged -= ContactItemsDidReceive;
            ContactsUpdated?.Invoke(this, null);

        }
        
        private string FormatPhoneNumber(string phoneNumber)
        {
            return string.IsNullOrEmpty(phoneNumber) ? phoneNumber : _phoneFormatter.Format(phoneNumber);
        }
    }
}