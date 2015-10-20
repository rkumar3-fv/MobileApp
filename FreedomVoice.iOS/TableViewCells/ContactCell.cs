using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ContactCell : UITableViewCell
    {
        public static readonly NSString ContactCellId = new NSString("ContactCell");

        public ContactCell() : base(UITableViewCellStyle.Subtitle, ContactCellId) { }
    }
}