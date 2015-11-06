﻿using System;
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

            var cell = tableView.DequeueReusableCell(FolderCell.FolderCellId) as FolderCell ?? new FolderCell { Accessory = UITableViewCellAccessory.DisclosureIndicator };

            cell.TextLabel.Text = folder.DisplayName;
            cell.ImageView.Image = FolderCell.GetImageByFolderName(folder.DisplayName);

            var unreadedMessagesCount = folder.UnreadMessagesCount;
            cell.NewMessagesCountLabel.Text = unreadedMessagesCount < 100 ? unreadedMessagesCount.ToString() : "99+";

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

            _navigationController.PushViewController(messagesController, false);
        }
    }
}