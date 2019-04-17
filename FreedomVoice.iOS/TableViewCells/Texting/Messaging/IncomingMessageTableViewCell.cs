using System;

using Foundation;
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
            // Note: this .ctor should not contain any initialization logic.
        }
        
        public string Text
        {
            set => TextLabel.Text = value;
        }
    }
}
