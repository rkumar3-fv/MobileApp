using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class RecentCell : UITableViewCell
    {
        public UILabel PhoneTitle { get; }
        public UILabel DialDate { get; }

        public static readonly NSString RecentCellId = new NSString("RecentCell");
        public RecentCell() : base(UITableViewCellStyle.Default, RecentCellId)
        {
            PhoneTitle = new UILabel { BackgroundColor = UIColor.Clear, Frame = new CGRect(15, 5, 220, 32) };
            DialDate = new UILabel 
            {
                TextColor = Theme.GrayColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Right,
                Frame = new CGRect(221, 5, 60, 32)
            };

            AddSubviews(PhoneTitle, DialDate);
        }
    }
}