using System.Collections.Generic;
using System.Threading.Tasks;
using Contacts;
using ContactsUI;
using Foundation;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Contacts
    {
        public async static Task<bool> CheckAccessStatus()
        {
            var store = new CNContactStore();
            var authorizationStatus = CNContactStore.GetAuthorizationStatus(CNEntityType.Contacts);

            switch (authorizationStatus)
            {
                case CNAuthorizationStatus.Authorized:
                    return true;
                case CNAuthorizationStatus.Denied:
                case CNAuthorizationStatus.NotDetermined:
                    var havePermissions = await store.RequestAccessAsync(CNEntityType.Contacts);
                    return havePermissions.Item1;
                default:
                    return false;
            }
        }

        public async static Task<List<CNContact>> GetContactsList()
        {
            var contactsList = new List<CNContact>();

            if (!await CheckAccessStatus()) return contactsList;

            NSError error;

            var request = new CNContactFetchRequest(CNContactViewController.DescriptorForRequiredKeys);

            var store = new CNContactStore();
            store.EnumerateContacts(request, out error, (cnContact, stop) => { contactsList.Add(cnContact); });

            return contactsList;
        }
    }
}