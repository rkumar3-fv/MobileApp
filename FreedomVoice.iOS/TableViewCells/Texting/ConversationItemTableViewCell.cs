using System;

using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells.Texting
{
    public partial class ConversationItemTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("ConversationItemTableViewCell");
        public static readonly UINib Nib;

        static ConversationItemTableViewCell()
        {
            Nib = UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle);
        }

        protected ConversationItemTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }
    }
}
