using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class MessagesSource : UITableViewSource
    {
        private readonly List<Message> _messages;

        private readonly Account _selectedAccount;
        private readonly ExtensionWithCount _selectedExtension;
        private readonly FolderWithCount _selectedFolder;

        private readonly UINavigationController _navigationController;

        public MessagesSource(List<Message> messages, ExtensionWithCount selectedExtension, Account selectedAccount, FolderWithCount folder, UINavigationController navigationController)
        {
            _messages = messages;

            _selectedExtension = selectedExtension;
            _selectedAccount = selectedAccount;
            _selectedFolder = folder;

            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var folder = _messages[indexPath.Row];

            var dateFormatter = new NSDateFormatter 
                                    {
                                        DoesRelativeDateFormatting = true,
                                        DateStyle = NSDateFormatterStyle.Short,
                                        TimeStyle = NSDateFormatterStyle.Short
                                    };

            var cell = tableView.DequeueReusableCell(FolderCell.FolderCellId) as FolderCell ?? new FolderCell();
            cell.TextLabel.Text = folder.Name;
            cell.DetailTextLabel.Text = dateFormatter.ToString(folder.ReceivedOn);
            cell.Accessory = UITableViewCellAccessory.None;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);

            var selectedMessage = _messages[indexPath.Row];
        }
    }
}