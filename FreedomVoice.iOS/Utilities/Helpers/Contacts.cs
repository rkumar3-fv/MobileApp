using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AddressBook;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Utilities.Extensions;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Contacts
    {
        private static readonly char[] Separators = { ' ', '-', '.', '/', '@' };

        static Contacts()
        {
            ContactNameFormat = ABPersonCompositeNameFormat.LastNameFirst;
            ContactSortOrder = ABPersonSortBy.LastName;
        }

        private static bool? HasContactsPermissions { get; set; }
        public static async Task<bool> ContactHasAccessPermissionsAsync()
        {
            return HasContactsPermissions ?? (HasContactsPermissions = await new Xamarin.Contacts.AddressBook().RequestPermission()).Value;
        }

        public static bool ContactsRequested { get; private set; }

        private static ABPersonCompositeNameFormat ContactNameFormat { get; set; }
        public static ABPersonSortBy ContactSortOrder { get; private set; }

        public async static Task<List<Contact>> GetContactsListAsync()
        {
            ContactsRequested = true;

            if (!await ContactHasAccessPermissionsAsync()) return new List<Contact>();

            ContactNameFormat = ABPerson.GetCompositeNameFormat(null);
            ContactSortOrder = ABPerson.SortOrdering;

            var contactsList = new Xamarin.Contacts.AddressBook().Where(c => c.Phones.Any()).ToList();
            contactsList.ForEach(c => UpdateDisplayName(c));

            return SortContacts(contactsList);
        }

        private static List<Contact> SortContacts(IEnumerable<Contact> contactsList)
        {
            IOrderedEnumerable<Contact> sortedContacts;

            switch (ContactSortOrder)
            {
                case ABPersonSortBy.LastName:
                    sortedContacts = ContactNameFormat == ABPersonCompositeNameFormat.LastNameFirst ? contactsList.OrderBy(c => c.DisplayName) : contactsList.OrderBy(c => c.LastName ?? c.FirstName ?? c.Emails.FirstOrDefault()?.Address ?? c.Phones.FirstOrDefault()?.Number);
                    break;
                case ABPersonSortBy.FirstName:
                    sortedContacts = ContactNameFormat == ABPersonCompositeNameFormat.FirstNameFirst ? contactsList.OrderBy(c => c.DisplayName) : contactsList.OrderBy(c => c.FirstName ?? c.LastName ?? c.Emails.FirstOrDefault()?.Address ?? c.Phones.FirstOrDefault()?.Number);
                    break;
                default:
                    sortedContacts = contactsList.OrderBy(c => c.DisplayName);
                    break;
            }

            return sortedContacts.ToList();
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
            if (normalizedSearchParts.All(phrase => c.Phones.Any(p => !string.IsNullOrEmpty(phrase) && DataFormatUtils.NormalizePhone(p.Number).Contains(phrase))))
                return true;

            if (string.IsNullOrEmpty(c.DisplayName)) return false;
            var fullNameParts = c.DisplayName.Split(Separators);

            return searchPhraseParts.All(phrase => fullNameParts.Any(part => part.StartsWith(phrase, StringComparison.OrdinalIgnoreCase)));
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
    }
}