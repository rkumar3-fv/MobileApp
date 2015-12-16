using System;
using System.Linq;
using FreedomVoice.Core.Utils;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Contacts
    {
        private static readonly char[] Separators = { ' ', '-', '.', '/', '@' };

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
                return string.IsNullOrEmpty(c.DisplayName) ? c.Phones.Any() : !char.IsLetter(c.DisplayName[0]);

            if (!string.IsNullOrEmpty(c.LastName) && c.LastName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return true;

            return (string.IsNullOrEmpty(c.LastName) && !string.IsNullOrEmpty(c.FirstName) && (c.FirstName.StartsWith(key, StringComparison.OrdinalIgnoreCase)));
        }
    }
}