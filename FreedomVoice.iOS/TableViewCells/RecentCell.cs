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
            PhoneTitle = new UILabel { BackgroundColor = UIColor.Clear };
            DialDate = new UILabel
            {
                TextColor = Theme.GrayColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Right
            };
        }

        public override void LayoutSubviews()
        {
            ContentView.AddSubviews(PhoneTitle, DialDate);

            base.LayoutSubviews();
        }
    }
}