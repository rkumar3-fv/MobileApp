using System;
using System.Linq;
using Foundation;
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
            if (lastName != null || displayName != null)
            {
                if (lastName != null && lastName.StartsWith(searchText, StringComparison.Ordinal) || displayName.StartsWith(searchText, StringComparison.Ordinal)) return;

                var phoneNumber = person.Phones.FirstOrDefault(phone => phone.Number.Contains(searchText))?.Number;
                if (!string.IsNullOrEmpty(phoneNumber))
                {
                    var grayColorRange = new NSRange(0, phoneNumber.Length);
                    var normalColorRange = new NSRange(phoneNumber.IndexOf(searchText, StringComparison.Ordinal), searchText.Length);

                    var detailTextAttributedString = new NSMutableAttributedString(phoneNumber);
                    detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.GrayColor, grayColorRange);
                    detailTextAttributedString.AddAttribute(UIStringAttributeKey.ForegroundColor, Theme.BlackColor, normalColorRange);

                    DetailTextLabel.AttributedText = detailTextAttributedString;
                }
            }

            if (lastName != null)
            {
                var boldedRange = new NSRange(displayName.IndexOf(lastName, StringComparison.Ordinal), lastName.Length);
                var textAttributedString = new NSMutableAttributedString(displayName);
                textAttributedString.AddAttribute(UIStringAttributeKey.Font, UIFont.SystemFontOfSize(17, UIFontWeight.Semibold), boldedRange);

                TextLabel.AttributedText = textAttributedString;
            }
            else
                TextLabel.Text = displayName;
        }
    }
}