using System;

using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting.Messaging
{
    public partial class MessageDateTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("MessageDateTableViewCell");
        public static readonly UINib Nib;

        static MessageDateTableViewCell()
        {
            Nib = UINib.FromName("MessageDateTableViewCell", NSBundle.MainBundle);
        }

        protected MessageDateTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public string Date
        {
            set => DateLabel.Text = value;
        }
    }
}
