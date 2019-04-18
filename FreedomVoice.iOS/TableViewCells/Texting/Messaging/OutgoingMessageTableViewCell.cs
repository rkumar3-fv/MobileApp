using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Views;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting.Messaging
{
    public partial class OutgoingMessageTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("OutgoingMessageTableViewCell");
        public static readonly UINib Nib;

        static OutgoingMessageTableViewCell()
        {
            Nib = UINib.FromName("OutgoingMessageTableViewCell", NSBundle.MainBundle);
        }

        protected OutgoingMessageTableViewCell(IntPtr handle) : base(handle)
        {
            var mask = new BubbleView(false);
            BubbleView.MaskView = mask;
        }

        public string Text
        {
            set => TextLabel.Text = value;
        }
    }
}
