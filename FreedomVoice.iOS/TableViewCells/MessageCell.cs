using Foundation;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class MessageCell : UITableViewCell
    {
        public static readonly NSString MessageCellId = new NSString("MessageCell");

        public MessageCell() : base(UITableViewCellStyle.Subtitle, MessageCellId) { }
    }
}