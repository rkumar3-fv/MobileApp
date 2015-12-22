﻿using System;
using System.Linq;
using AddressBook;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Extensions;
using UIKit;
using Xamarin.Contacts;
using ContactsHelper = FreedomVoice.iOS.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ContactCell : UITableViewCell
    {
        public static readonly NSString ContactCellId = new NSString("ContactCell");

        public ContactCell() : base(UITableViewCellStyle.Subtitle, ContactCellId) { }

        internal void UpdateCell(Contact person, string searchText)
        {
            var displayName = person.DisplayName;

            var firstName = person.FirstName;
            var lastName = person.LastName;
            var middleName = person.MiddleName;

            var textToMakeBold = ContactsHelper.ContactSortOrder == ABPersonSortBy.LastName ? lastName : firstName;

            var boldedRange = string.IsNullOrEmpty(textToMakeBold) ? new NSRange(0, displayName.Length)
                                                                   : new NSRange(displayName.IndexOf(textToMakeBold, StringComparison.Ordinal), textToMakeBold.Length);

            var textAttributedString = new NSMutableAttributedString(displayName);
            textAttributedString.AddAttribute(UIStringAttributeKey.Font, UIFont.SystemFontOfSize(17, UIFontWeight.Semibold), boldedRange);

            TextLabel.AttributedText = textAttributedString;

            if (string.IsNullOrEmpty(searchText) || firstName.NotNullAndStartsWith(searchText) || lastName.NotNullAndStartsWith(searchText) || middleName.NotNullAndStartsWith(searchText))
            {
                DetailTextLabel.Text = null;
                return;
            }

            var emailAddress = person.Emails.FirstOrDefault(p => p.Address.Contains(searchText))?.Address;
            if (!string.IsNullOrEmpty(emailAddress))
            {
                DetailTextLabel.AttributedText = GetDetailTextAttributedString(emailAddress, searchText);
                return;
            }

            var normalizedSearchText = DataFormatUtils.NormalizePhone(searchText);
            var phoneNumber = person.Phones.FirstOrDefault(p => !string.IsNullOrEmpty(normalizedSearchText) && DataFormatUtils.NormalizePhone(p.Number).Contains(normalizedSearchText))?.Number;

            if (!string.IsNullOrEmpty(phoneNumber))
            {
                DetailTextLabel.AttributedText = GetDetailTextAttributedString(DataFormatUtils.NormalizePhone(phoneNumber), normalizedSearchText);
                return;
            }

            DetailTextLabel.Text = null;
        }

        private static NSMutableAttributedString GetDetailTextAttributedString(string text, string searchText)
        {
            var grayColorRange = new NSRange(0, text.Length);
            var normalColorRange = new NSRange(text.IndexOf(searchText, StringComparison.Ordinal), searchText.Length);

            var detailTextAttributedString = new NSMutableAttributedString(text);
            detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.GrayColor, grayColorRange);
            detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.BlackColor, normalColorRange);

            return detailTextAttributedString;
        }
    }
}