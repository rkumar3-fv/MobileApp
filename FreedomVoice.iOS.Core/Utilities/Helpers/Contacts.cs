using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AddressBook;
using Contacts;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.iOS.Core.Utilities.Extensions;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Core.Utilities.Helpers
{
    public class FVPhone
    {
        public string Label { get; set; }

        public string Number { get; set; }
    }
    
    public class FVContact
    {
        public string DisplayName
        {
            get
            {
                var displayName = "";
                if (!string.IsNullOrEmpty(FirstName))
                {
                    displayName = FirstName;
                }

                if (!string.IsNullOrEmpty(MiddleName))
                {
                    if (!string.IsNullOrEmpty(displayName))
                        displayName += " " + MiddleName;
                    else
                        displayName = MiddleName;
                }

                if (!string.IsNullOrEmpty(LastName))
                {
                    if (!string.IsNullOrEmpty(displayName))
                        displayName += " " + LastName;
                    else
                        displayName = LastName;
                }

                return displayName;
            }
        }

        public string FirstName { get; set; }

        public string MiddleName { get; set; }

        public string LastName { get; set; }

        public IEnumerable<FVPhone> Phones { get; set; }
        
    }

    public static class Contacts
    {
        private static readonly char[] Separators = { ' ', '-', '.', '/', '@' };

        public static List<Contact> ContactList { get; private set; }

        public static event EventHandler ItemsChanged;

        public static int ContactsCount => ContactList.Count;

        static Contacts()
        {
            ContactList = new List<Contact>();

            ContactNameFormat = ABPersonCompositeNameFormat.LastNameFirst;
            ContactSortOrder = ABPersonSortBy.LastName;
        }

        private static bool? HasContactsPermissions { get; set; }
        public static async Task<bool> ContactHasAccessPermissionsAsync()
        {
            return HasContactsPermissions ?? (HasContactsPermissions = await new Xamarin.Contacts.AddressBook().RequestPermission()).Value;
        }

        private static bool ContactsRequested { get; set; }

        private static ABAddressBook NotificationAddressBook { get; set; }
        private static CNContactStore NotificationContactStore { get; set; }

        private static ABPersonCompositeNameFormat ContactNameFormat { get; set; }
        public static ABPersonSortBy ContactSortOrder { get; private set; }

        public static void SubscribeToContactsChange()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
            {
                NotificationContactStore = new CNContactStore();
                NSNotificationCenter.DefaultCenter.AddObserver(CNContactStore.NotificationDidChange, MyAddressBookExternalChangeCallback);
            }
            else
            {
                NotificationAddressBook = GetAddressBook();
                if (NotificationAddressBook == null)
                    return;

                NotificationAddressBook.ExternalChange += MyAddressBookExternalChangeCallback;
            }
        }

        public static ABAddressBook GetAddressBook()
        {
            var authorizationStatus = ABAddressBook.GetAuthorizationStatus();
            if (authorizationStatus != ABAuthorizationStatus.Authorized)
                return null;

            NSError error;

            return ABAddressBook.Create(out error);
        }

        private static void MyAddressBookExternalChangeCallback(NSNotification notification)
        {
            RenewContactsList();
        }

        private static void MyAddressBookExternalChangeCallback(object sender, ExternalChangeEventArgs e)
        {
            RenewContactsList();
        }

        private static async void RenewContactsList()
        {
            ContactsRequested = false;

            await GetContactsListAsync();
        }

        public static void GetContactsList(Action<IEnumerable<FVContact>> completed)
        {
            var requiredKeys = new [] { CNContactKey.GivenName, CNContactKey.FamilyName, CNContactKey.MiddleName, CNContactKey.PhoneNumbers, CNContactKey.Type };
            var defaultContainerIdentifier = new CNContactStore().DefaultContainerIdentifier;

            var contactStore = new CNContactStore();
            var contactList = contactStore.GetUnifiedContacts(CNContact.GetPredicateForContactsInContainer(defaultContainerIdentifier), requiredKeys, out var error);

            List<FVContact> contacts = new List<FVContact>();

            Console.WriteLine($"Contact list form book : {contactList.Count()}");

            foreach (var cnContact in contactList)
            {
                var phones = new List<FVPhone>();
                
                if (cnContact.PhoneNumbers != null)
                    foreach (var p in cnContact.PhoneNumbers)
                    {
                        phones.Add(new FVPhone
                        {
                            Label = p.Label,
                            Number = p.Value.StringValue,
                        });
                    }

                contacts.Add(new FVContact
                {
                    Phones = phones,
                    MiddleName = cnContact.MiddleName,
                    FirstName = cnContact.GivenName,
                    LastName = cnContact.FamilyName,
                });
            }

            Console.WriteLine($"Contact list form book : {contacts.Count()}");
            completed?.Invoke(contacts);
        }
        
        public async static Task GetContactsListAsync()
        {
            if (ContactsRequested)
                return;

            ContactsRequested = true;

            if (!await ContactHasAccessPermissionsAsync())
                return;

            ContactNameFormat = ABPerson.GetCompositeNameFormat(null);
            ContactSortOrder = ABPerson.SortOrdering;

            var contactsList = new Xamarin.Contacts.AddressBook().Where(c => c.Phones.Any()).ToList();
            contactsList.ForEach(c => UpdateDisplayName(c));

            ContactList = SortContacts(contactsList);
            ItemsChanged?.Invoke(null, null);
        }

        private static List<Contact> SortContacts(IEnumerable<Contact> contactsList)
        {
            IOrderedEnumerable<Contact> sortedContacts;

            switch (ContactSortOrder)
            {
                case ABPersonSortBy.LastName:
                    sortedContacts = ContactNameFormat == ABPersonCompositeNameFormat.LastNameFirst ? contactsList.OrderBy(c => c.DisplayName) : contactsList.OrderBy(c => c.LastName ?? c.FirstName ?? c.Emails?.FirstOrDefault()?.Address ?? c.Phones?.FirstOrDefault()?.Number);
                    break;
                case ABPersonSortBy.FirstName:
                    sortedContacts = ContactNameFormat == ABPersonCompositeNameFormat.FirstNameFirst ? contactsList.OrderBy(c => c.DisplayName) : contactsList.OrderBy(c => c.FirstName ?? c.LastName ?? c.Emails?.FirstOrDefault()?.Address ?? c.Phones?.FirstOrDefault()?.Number);
                    break;
                default:
                    sortedContacts = contactsList.OrderBy(c => c.DisplayName);
                    break;
            }

            return GetMergedContacts(sortedContacts);
        }

        private static List<Contact> GetMergedContacts(IOrderedEnumerable<Contact> contacts)
        {
            var mergedContacts = new List<Contact>();

            var groups = contacts.GroupBy(c => c.DisplayName);
            Console.WriteLine(groups.Count());
            foreach (var @group in groups)
            {
                if (@group.Count() == 1)
                {
                    mergedContacts.Add(@group.First());
                    continue;
                }

                var firstContact = @group.First();
                foreach (var contact in @group.Skip(1))
                {
                    if (contact.FirstName != firstContact.FirstName || contact.LastName != firstContact.LastName)
                    {
                        mergedContacts.Add(contact);
                        continue;
                    }

                    var hasMatchedPhone = contact.Phones.Any(phone => firstContact.Phones.Any(p => NormalizePhoneNumber(p.Number) == NormalizePhoneNumber(phone.Number)));
                    if (hasMatchedPhone)
                    {
                        var firstContactPhones = firstContact.Phones.ToList();

                        firstContactPhones.AddRange(contact.Phones.Where(phone => firstContact.Phones.All(p => NormalizePhoneNumber(p.Number) != NormalizePhoneNumber(@phone.Number))));
                        firstContact.Phones = firstContactPhones;
                    }
                    else
                        mergedContacts.Add(contact);
                }

                mergedContacts.Add(firstContact);
            }

            return mergedContacts;
        }

        private static string UpdateDisplayName(Contact c)
        {
            if (string.IsNullOrEmpty(c.DisplayName))
                return c.DisplayName = c.Emails.Any() ? c.Emails.First().Address : c.Phones.First().Number;

            var lastName = c.LastName;
            var firstName = c.FirstName;
            var middleName = c.MiddleName;

            if (!string.IsNullOrEmpty(c.DisplayName) && string.IsNullOrEmpty(lastName) && string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(middleName))
                return c.DisplayName;

            return c.DisplayName = ContactNameFormat == ABPersonCompositeNameFormat.LastNameFirst ? $"{lastName ?? ""} {firstName ?? ""} {middleName ?? ""}".Trim() : $"{firstName ?? ""} {lastName ?? ""} {middleName ?? ""}".Trim();
        }

        public static bool ContactMatchPredicate(Contact c, string searchText)
        {
            var searchPhraseParts = searchText.Split(Separators);

            var normalizedSearchParts = searchPhraseParts.Select(DataFormatUtils.NormalizeSearchText).ToArray();
            if (normalizedSearchParts.All(phrase => c.Phones.Any(p => !string.IsNullOrEmpty(phrase) && NormalizePhoneNumber(p.Number).Contains(phrase))))
                return true;

            if (string.IsNullOrEmpty(c.DisplayName)) return false;
            var fullNameParts = c.DisplayName.Split(Separators);

            return searchPhraseParts.All(phrase => fullNameParts.Any(part => part.Contains(phrase, StringComparison.OrdinalIgnoreCase)));
        }

        public static bool ContactSearchPredicate(Contact c, string key)
        {
            if (key == "#")
            {
                if (string.IsNullOrEmpty(c.DisplayName))
                    return c.Phones.Any();

                return !char.IsLetter(ContactSortOrder == ABPersonSortBy.LastName ? (string.IsNullOrEmpty(c.LastName) ? c.DisplayName[0] : c.LastName[0]) : (string.IsNullOrEmpty(c.FirstName) ? c.DisplayName[0] : c.FirstName[0]));
            }

            if (string.IsNullOrEmpty(c.FirstName) && string.IsNullOrEmpty(c.LastName))
                return c.DisplayName.NotNullAndStartsWith(key);

            if (ContactSortOrder == ABPersonSortBy.LastName)
            {
                if (c.LastName.NotNullAndStartsWith(key))
                    return true;

                return string.IsNullOrEmpty(c.LastName) && c.FirstName.NotNullAndStartsWith(key);
            }

            if (c.FirstName.NotNullAndStartsWith(key))
                return true;

            return string.IsNullOrEmpty(c.FirstName) && c.LastName.NotNullAndStartsWith(key);
        }

        public static string NormalizePhoneNumber(string number)
        {
            return string.IsNullOrEmpty(number) ? number : ServiceContainer.Resolve<IPhoneFormatter>().NormalizeNational(number);
        }

        public static Contact FindContactByNumber(string number)
        {
            return ContactList.FirstOrDefault(c => c.Phones.Any(p => NormalizePhoneNumber(p.Number) == NormalizePhoneNumber(number)));
        }
    }
}