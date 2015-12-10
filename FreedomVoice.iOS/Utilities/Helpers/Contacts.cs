﻿using System;
using System.Linq;
using FreedomVoice.Core.Utils;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Contacts
    {
        private static readonly char[] Separators = { ' ', '-' };

        public static bool ContactMatchPredicate(Contact c, string searchText)
        {
            var normalizedSearchText = DataFormatUtils.NormalizePhone(searchText);
            if (c.Phones.Any(p => !string.IsNullOrEmpty(normalizedSearchText) && DataFormatUtils.NormalizePhone(p.Number).Contains(normalizedSearchText)))
                return true;

            if (string.IsNullOrEmpty(c.DisplayName)) return false;

            var searchPhraseParts = searchText.Split(Separators);
            var fullNameParts = c.DisplayName.Split(Separators);

            return searchPhraseParts.All(phrase => fullNameParts.Any(part => part.StartsWith(phrase, StringComparison.OrdinalIgnoreCase)));
        }

        public static bool ContactSearchPredicate(Contact c, string key)
        {
            if (key == "#")
                return string.IsNullOrEmpty(c.DisplayName) && c.Phones.Any();

            if (!string.IsNullOrEmpty(c.LastName) && c.LastName.StartsWith(key, StringComparison.OrdinalIgnoreCase))
                return true;

            return (string.IsNullOrEmpty(c.LastName) && !string.IsNullOrEmpty(c.FirstName) && (c.FirstName.StartsWith(key, StringComparison.OrdinalIgnoreCase)));
        }
    }
}