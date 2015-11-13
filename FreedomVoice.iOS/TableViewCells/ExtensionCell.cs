using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExtensionCell : UITableViewCell
    {
        private readonly UILabel _newMessagesCountLabel;

        public static readonly NSString ExtensionCellId = new NSString("ExtensionCell");
        public ExtensionCell() : base(UITableViewCellStyle.Default, ExtensionCellId)
        {
            _newMessagesCountLabel = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 12.5, 30, 19))
            {
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };

            _newMessagesCountLabel.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            _newMessagesCountLabel.Layer.CornerRadius = 3;

            Add(_newMessagesCountLabel);
        }

        public void UpdateCell(ExtensionWithCount extension)
        {
            TextLabel.Text = string.Concat(extension.ExtensionNumber, " - ", extension.DisplayName);

            var unreadedMessagesCount = extension.UnreadMessagesCount;
            _newMessagesCountLabel.Hidden = unreadedMessagesCount == 0;
            _newMessagesCountLabel.Text = unreadedMessagesCount < 100 ? unreadedMessagesCount.ToString() : "99+";
        }
    }
}