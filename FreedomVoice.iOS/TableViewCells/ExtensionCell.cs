using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExtensionCell : UITableViewCell
    {
        public UILabel NewMessagesCountLabel { get; }

        public static readonly NSString ExtensionCellId = new NSString("ExtensionCell");
        public ExtensionCell() : base(UITableViewCellStyle.Default, ExtensionCellId)
        {
            NewMessagesCountLabel = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 13, 30, 19))
            {
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };

            NewMessagesCountLabel.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            NewMessagesCountLabel.Layer.CornerRadius = 3;

            Add(NewMessagesCountLabel);
        }
    }
}