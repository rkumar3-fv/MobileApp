using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;
using AppearanceHelper = FreedomVoice.iOS.Utilities.Helpers.Appearance;

namespace FreedomVoice.iOS.TableViewCells
{
    public class MessageCell : UITableViewCell
    {
        #region Controls

        private readonly UIImageView _image;

        private readonly UILabel _title;
        private readonly UILabel _date;
        private readonly UILabel _length;

        #endregion

        public static readonly NSString MessageCellId = new NSString("MessageCell");
        public MessageCell() : base(UITableViewCellStyle.Default, MessageCellId)
        {
            _image = AppearanceHelper.GetMessageImageView(48);
            _title = new UILabel(new CGRect(48, 9, Theme.ScreenBounds.Width - 63, 21)) { Font = UIFont.SystemFontOfSize(17) };
            _length = new UILabel(new CGRect(Theme.ScreenBounds.Width - 75, 30, 60, 15)) { Font = UIFont.SystemFontOfSize(12), TextAlignment = UITextAlignment.Right };
            _date = new UILabel(new CGRect(48, 30, _length.Frame.X - 53, 15)) { Font = UIFont.SystemFontOfSize(12) };

            AddSubviews(_image, _title, _date, _length);
        }

        public void UpdateCell(Message message)
        {
            _image.Image = AppearanceHelper.GetMessageImage(message.Type, message.Unread, false);

            _title.Text = message.Title;
            _title.Font = UIFont.SystemFontOfSize(17, message.Unread ? UIFontWeight.Bold : UIFontWeight.Regular);
            _title.Center = new CGPoint(_title.Center.X, 16);

            _date.Text = DataFormatUtils.ToFormattedDate("Yesterday", message.ReceivedOn);
            _date.Center = new CGPoint(_date.Center.X, 35);
            
            _length.Text = AppearanceHelper.GetFormattedMessageLength(message.Length, message.Type == MessageType.Fax);
            _length.Center = new CGPoint(_length.Center.X, 35);
        }
    }
}