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
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            base.AwakeFromNib();
            var image = new UIImage("bubble_sent.png")
                .CreateResizableImage(new UIEdgeInsets(17, 21, 17, 21), UIImageResizingMode.Stretch)
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            BubbleView.Image = image;
            BubbleView.TintColor = new UIColor(0.37f, 0.81f, 0.36f, 1.0f);
        }

        public string Text
        {
            set => MessageLabel.Text = value;
        }
    }
}
