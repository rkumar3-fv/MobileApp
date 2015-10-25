using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class FolderCell : UITableViewCell
    {
        public static readonly NSString FolderCellId = new NSString("FolderCell");

        public FolderCell() : base(UITableViewCellStyle.Value1, FolderCellId) { }
    }
}
