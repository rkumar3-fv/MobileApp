using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewCells
{
    public class FolderCell : UITableViewCell
    {
        public UILabel NewMessagesCountLabel { get; }

        public static readonly NSString FolderCellId = new NSString("FolderCell");
        public FolderCell() : base(UITableViewCellStyle.Default, FolderCellId)
        {
            NewMessagesCountLabel = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 13, 30, 19))
            {
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };

            NewMessagesCountLabel.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            NewMessagesCountLabel.Layer.CornerRadius = 3;

            Add(NewMessagesCountLabel);
        }

        public static UIImage GetImageByFolderName(string folderName)
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