using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contacts;
using ContactsUI;
using Foundation;
using FreedomVoice.Core.Utils;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Contacts
    {
        //public async static Task<bool> CheckAccessStatus()
        //{
        //    var store = new CNContactStore();
        //    var authorizationStatus = CNContactStore.GetAuthorizationStatus(CNEntityType.Contacts);

        //    switch (authorizationStatus)
        //    {
        //        case CNAuthorizationStatus.Authorized:
        //            return true;
        //        case CNAuthorizationStatus.Denied:
        //        case CNAuthorizationStatus.NotDetermined:
        //            var havePermissions = await store.RequestAccessAsync(CNEntityType.Contacts);
        //            return havePermissions.Item1;
        //        default:
        //            return false;
        //    }
        //}

        //public async static Task<List<CNContact>> GetContactsList()
        //{
        //    var contactsList = new List<CNContact>();

        //    if (!await CheckAccessStatus()) return contactsList;

        //    var request = new CNContactFetchRequest(CNContactViewController.DescriptorForRequiredKeys);

        //    var store = new CNContactStore();
        //    NSError error;
        //    store.EnumerateContacts(request, out error, (cnContact, stop) => { contactsList.Add(cnContact); });

        //    return contactsList;
        //}

        public static bool ContactMatchPredicate(Contact c, string searchText)
        {
            if (!string.IsNullOrEmpty(c.LastName) && c.LastName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                return true;

            if ((string.IsNullOrEmpty(c.LastName) || !c.LastName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)) && !string.IsNullOrEmpty(c.DisplayName) && (c.DisplayName.StartsWith(searchText, StringComparison.OrdinalIgnoreCase)))
                return true;

            return c.Phones.Any(p => DataFormatUtils.NormalizePhone(p.Number).Contains(DataFormatUtils.NormalizePhone(searchText)));
        }

        public static bool ContactSearchPredicate(Contact c, string key)
        {
            if (key == "#")
                return string.IsNullOrEmpty(c.DisplayName) && c.Phones.Any();

            if (!string.IsNullOrEmpty(c.LastName) && c.LastName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return true;

            return (string.IsNullOrEmpty(c.LastName) && !string.IsNullOrEmpty(c.DisplayName) && (c.DisplayName.StartsWith(key, StringComparison.OrdinalIgnoreCase)));
        }
    }
}