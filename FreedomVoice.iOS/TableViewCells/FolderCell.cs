using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using UIKit;
using AppearanceHelper = FreedomVoice.iOS.Utilities.Helpers.Appearance;

namespace FreedomVoice.iOS.TableViewCells
{
    public class FolderCell : UITableViewCell
    {
        private readonly UIImageView _imageView;

        private readonly UILabel _titleLabel;
        private readonly UILabel _newMessagesCountLabel;

        public static readonly NSString FolderCellId = new NSString("FolderCell");
        public FolderCell() : base(UITableViewCellStyle.Default, FolderCellId)
        {
            _imageView = AppearanceHelper.GetMessageImageView(44);
            _titleLabel = new UILabel(new CGRect(48, 12, Theme.ScreenBounds.Width - 134, 21)) { Font = UIFont.SystemFontOfSize(17) };

            _newMessagesCountLabel = new UILabel(new CGRect(Theme.ScreenBounds.Width - 69, 12.5, 30, 19))
            {
                TextColor = UIColor.Black,
                Font = UIFont.SystemFontOfSize(12),
                TextAlignment = UITextAlignment.Center,
                ClipsToBounds = true
            };

            _newMessagesCountLabel.Layer.BackgroundColor = UIColor.FromRGBA(0, 0, 0, 25).CGColor;
            _newMessagesCountLabel.Layer.CornerRadius = 3;

            AddSubviews(_imageView, _titleLabel, _newMessagesCountLabel);
        }

        public void UpdateCell(FolderWithCount folder)
        {
            _titleLabel.Text = folder.DisplayName;

            _imageView.Image = GetImageByFolderName(folder.DisplayName);

            var unreadedMessagesCount = folder.UnreadMessagesCount;
            _newMessagesCountLabel.Hidden = unreadedMessagesCount == 0;
            _newMessagesCountLabel.Text = unreadedMessagesCount < 100 ? unreadedMessagesCount.ToString() : "99+";
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