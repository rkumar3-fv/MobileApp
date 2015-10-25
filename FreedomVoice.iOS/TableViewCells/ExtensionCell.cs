using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class ExtensionCell : UITableViewCell
    {
        public static readonly NSString ExtensionCellId = new NSString("ExtensionCell");

        public ExtensionCell() : base(UITableViewCellStyle.Value1, ExtensionCellId) { }
    }
}