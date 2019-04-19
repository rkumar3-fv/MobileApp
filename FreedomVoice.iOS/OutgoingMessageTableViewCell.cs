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
        private BubbleView _bubbleView;

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
            MessageLabel.Lines = 0;
            SelectionStyle = UITableViewCellSelectionStyle.None;
            MessageLabel.Su
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            var bubbleSize = new CGSize(MessageLabel.Superview.Frame.Width + 20, MessageLabel.Superview.Frame.Height + 20);


            //let bubbleView = BubbleView()
            _bubbleView.Frame = new CGRect(-14, -10, bubbleSize.Width, bubbleSize.Height);
            _bubbleView.Center = MessageLabel.Superview.Center;
            _bubbleView.BackgroundColor = UIColor.Clear;
        }

        public string Text
        {
            set => MessageLabel.Text = value;
        }
    }
}
