using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class FoldersSource : UITableViewSource
    {
        private readonly List<FolderWithCount> _folders;

        private readonly Account _selectedAccount;
        private readonly ExtensionWithCount _selectedExtension;
        
        private readonly UINavigationController _navigationController;

        public FoldersSource(List<FolderWithCount> folders, ExtensionWithCount selectedExtension, Account selectedAccount, UINavigationController navigationController)
        {
            _folders = folders;
            _selectedExtension = selectedExtension;
            _selectedAccount = selectedAccount;
            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var folder = _folders[indexPath.Row];

            var cell = tableView.DequeueReusableCell(FolderCell.FolderCellId) as FolderCell ?? new FolderCell();
            cell.TextLabel.Text = folder.DisplayName;
            cell.DetailTextLabel.Text = folder.UnreadMessagesCount.ToString();
            cell.ImageView.Image = GetImageByFolderName(folder.DisplayName);
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _folders?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedFolder = _folders[indexPath.Row];

            var messagesController = AppDelegate.GetViewController<MessagesViewController>();
            messagesController.SelectedAccount = _selectedAccount;
            messagesController.SelectedExtension = _selectedExtension;
            messagesController.SelectedFolder = selectedFolder;

            _navigationController.PushViewController(messagesController, true);
        }

        private UIImage GetImageByFolderName(string folderName)
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