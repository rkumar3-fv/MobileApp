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
            const string pattern = @"\d"; 
        
            var sb = new StringBuilder(); 
            foreach (Match m in Regex.Matches(formattedNumber, pattern)) 
                sb.Append(m); 

            return sb.ToString();
        }

        private void ContactItemsDidReceive(object sender, EventArgs e)
        {
            foreach(var contact in Helpers.Contacts.ContactList)
            {
                foreach(var phone in contact.Phones) {
                    var raw = Regex.Replace(phone.Number, @"\D", "");
                    _contactNames[raw] = contact.DisplayName;
                }

            }
            Contacts.ItemsChanged -= ContactItemsDidReceive;
            ContactsUpdated?.Invoke(this, null);

        }
        
        private string FormatPhoneNumber(string phoneNumber)
        {
            return string.IsNullOrEmpty(phoneNumber) ? phoneNumber : FreedomVoice.Core.Utils.ServiceContainer.Resolve<IPhoneFormatter>().Format(phoneNumber);
        }
    }
}