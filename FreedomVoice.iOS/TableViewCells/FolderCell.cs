using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class FolderCell : UITableViewCell
    {
        public static readonly NSString FolderCellId = new NSString("FolderCell");

        public FolderCell() : base(UITableViewCellStyle.Default, FolderCellId) { }

        public void UpdateCell(FolderWithCount folder)
        {
            Accessory = UITableViewCellAccessory.DisclosureIndicator;
            TextLabel.Text = folder.DisplayName;
            ImageView.Image = GetImageByFolderName(folder.DisplayName);

            var unreadedMessagesCount = folder.UnreadMessagesCount;

            var details = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 13, 30, 19))
            {
                Text = unreadedMessagesCount < 100 ? unreadedMessagesCount.ToString() : "99+",
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };
            details.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            details.Layer.CornerRadius = 3;
            details.Center = new CGPoint(details.Center.X, Center.Y);

            AddSubview(details);
        }

        private static UIImage GetImageByFolderName(string folderName)
        {
            switch (folderName)
            {
                case "New":
                    return UIImage.FromFile("folder_new.png");
                case "Saved":
                    return UIImage.FromFile("folder_saved.png");
                case "Trash":
                    return UIImage.FromFile("folder_trash.png");
                case "Sent":
                    return UIImage.FromFile("folder_sent.png");
                default:
                    return UIImage.FromFile("folder_new.png");
            }
        }
    }
}