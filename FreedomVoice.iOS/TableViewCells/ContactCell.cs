using System;
using System.Linq;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Utilities;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ContactCell : UITableViewCell
    {
        public static readonly NSString ContactCellId = new NSString("ContactCell");

        public ContactCell() : base(UITableViewCellStyle.Subtitle, ContactCellId) { }

        internal void UpdateCell(Contact person, string searchText)
        {
            var displayName = person.DisplayName;
            var lastName = person.LastName;

            if (string.IsNullOrEmpty(searchText))
                DetailTextLabel.Text = null;
            else 
            if (!string.IsNullOrEmpty(lastName) || !string.IsNullOrEmpty(displayName) || person.Phones.Any())
            {
                if (!string.IsNullOrEmpty(lastName) && lastName.StartsWith(searchText, StringComparison.Ordinal) || !string.IsNullOrEmpty(displayName) && displayName.StartsWith(searchText, StringComparison.Ordinal)) return;

                var normalizedSearchString = DataFormatUtils.NormalizePhone(searchText);

                var phoneNumber = person.Phones.FirstOrDefault(phone => DataFormatUtils.NormalizePhone(phone.Number).Contains(normalizedSearchString))?.Number;

                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    var normalizedPhone = DataFormatUtils.NormalizePhone(phoneNumber);

                    var grayColorRange = new NSRange(0, normalizedPhone.Length);
                    var normalColorRange = new NSRange(normalizedPhone.IndexOf(normalizedSearchString, StringComparison.Ordinal), normalizedSearchString.Length);

                    var detailTextAttributedString = new NSMutableAttributedString(normalizedPhone);
                    detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.GrayColor, grayColorRange);
                    detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.BlackColor, normalColorRange);

                    DetailTextLabel.AttributedText = detailTextAttributedString;
                }
                else
                    DetailTextLabel.Text = null;
            }

            if (!string.IsNullOrEmpty(displayName))
            {
                var boldedRange = string.IsNullOrEmpty(lastName) ? new NSRange(0, displayName.Length)
                                                                 : new NSRange(displayName.IndexOf(lastName, StringComparison.Ordinal), lastName.Length);

                var textAttributedString = new NSMutableAttributedString(displayName);
                textAttributedString.AddAttribute(UIStringAttributeKey.Font, UIFont.SystemFontOfSize(17, UIFontWeight.Semibold), boldedRange);

                TextLabel.AttributedText = textAttributedString;
            }
            else
                TextLabel.AttributedText = new NSAttributedString(person.Phones.First().Number, new UIStringAttributes { Font = UIFont.SystemFontOfSize(17, UIFontWeight.Semibold) });
        }
    }
}