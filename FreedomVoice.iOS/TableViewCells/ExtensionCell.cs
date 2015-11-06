using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExtensionCell : UITableViewCell
    {
        public static readonly NSString ExtensionCellId = new NSString("ExtensionCell");

        public ExtensionCell() : base(UITableViewCellStyle.Default, ExtensionCellId) { }

        public void UpdateCell(ExtensionWithCount extension)
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            TextLabel.Text = string.Concat(extension.ExtensionNumber, " - ", extension.DisplayName);

            var unreadedMessagesCount = extension.UnreadMessagesCount;

            var details = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 13, 30, 19))
            {
                Text = unreadedMessagesCount < 100 ? unreadedMessagesCount.ToString() : "99+",
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };
            details.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            details.Layer.CornerRadius = 3;
            details.Center = new CGPoint(details.Center.X, Center.Y);

            AddSubview(details);
        }
    }
}