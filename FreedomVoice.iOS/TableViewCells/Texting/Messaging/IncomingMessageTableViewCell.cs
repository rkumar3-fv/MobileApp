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
        }


        public string Text
        {
            set => MessageLabel.Text = value;
        }
    }
}
