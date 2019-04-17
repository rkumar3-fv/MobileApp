using System;

using Foundation;
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
            // Note: this .ctor should not contain any initialization logic.
        }

        public string Text
        {
            set => TextLabel.Text = value;
        }
    }
}
