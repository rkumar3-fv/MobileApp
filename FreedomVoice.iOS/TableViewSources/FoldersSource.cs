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
        public List<FolderWithCount> Folders { private get; set; }

        private readonly Account _selectedAccount;
        private readonly ExtensionWithCount _selectedExtension;
        
        private readonly UINavigationController _navigationController;

        public FoldersSource(List<FolderWithCount> folders, ExtensionWithCount selectedExtension, Account selectedAccount, UINavigationController navigationController)
        {
            Folders = folders;
            _selectedExtension = selectedExtension;
            _selectedAccount = selectedAccount;
            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var folder = Folders[indexPath.Row];

            var cell = tableView.DequeueReusableCell(FolderCell.FolderCellId) as FolderCell ?? new FolderCell { Accessory = UITableViewCellAccessory.DisclosureIndicator };
            cell.UpdateCell(folder);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return Folders?.Count ?? 0;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 44;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedFolder = Folders[indexPath.Row];

            var messagesController = AppDelegate.GetViewController<MessagesViewController>();
            messagesController.SelectedAccount = _selectedAccount;
            messagesController.SelectedExtension = _selectedExtension;
            messagesController.SelectedFolder = selectedFolder;

            _navigationController.PushViewController(messagesController, false);
        }
    }
}