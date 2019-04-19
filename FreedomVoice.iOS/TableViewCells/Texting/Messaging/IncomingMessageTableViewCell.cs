using System;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Views;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting.Messaging
{
    public partial class IncomingMessageTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("IncomingMessageTableViewCell");
        public static readonly UINib Nib;

        static IncomingMessageTableViewCell()
        {
            Nib = UINib.FromName("IncomingMessageTableViewCell", NSBundle.MainBundle);
        }

        protected IncomingMessageTableViewCell(IntPtr handle) : base(handle)
        {
        }

        [Export("awakeFromNib")]
        public void AwakeFromNib()
        {
            base.AwakeFromNib();
            var image = new UIImage("bubble_received.png")
                .CreateResizableImage(new UIEdgeInsets(17, 21, 17, 21), UIImageResizingMode.Stretch)
                .ImageWithRenderingMode(UIImageRenderingMode.AlwaysTemplate);
            BubbleView.Image = image;
            BubbleView.TintColor = new UIColor(0.90f, 0.90f, 0.91f, 1.0f);
            
        }


        public string Text
        {
            set => MessageLabel.Text = value;
        }
    }
}
