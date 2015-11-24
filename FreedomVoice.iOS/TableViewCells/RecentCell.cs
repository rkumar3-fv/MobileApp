using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class RecentCell : UITableViewCell
    {
        private UILabel PhoneTitle { get; }
        private UILabel DialDate { get; }

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

        public void UpdateCell(Recent recent)
        {
            Accessory = string.IsNullOrEmpty(recent.Title) ? UITableViewCellAccessory.None : UITableViewCellAccessory.DetailButton;

            var dialDateLabelText = recent.FormatedDialDate;
            var dialDateLabelWidth = ((NSString)dialDateLabelText).StringSize(UIFont.SystemFontOfSize(12)).Width;
            var dialDatePositionX = Theme.ScreenBounds.Width - dialDateLabelWidth - 45;

            DialDate.Frame = new CGRect(dialDatePositionX, 14, dialDateLabelWidth, 16);
            PhoneTitle.Frame = new CGRect(15, 11, dialDatePositionX - 20, 22);

            PhoneTitle.Text = recent.TitleOrNumber;
            DialDate.Text = dialDateLabelText;
        }

        public override void LayoutSubviews()
        {
            ContentView.AddSubviews(PhoneTitle, DialDate);

            base.LayoutSubviews();
        }
    }
}