using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class AccountCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("AccoundCell");

        public AccountCell() : base(UITableViewCellStyle.Default, Key) { }
    }
}