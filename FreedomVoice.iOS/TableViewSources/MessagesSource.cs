using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using UIKit;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Helpers;

namespace FreedomVoice.iOS.TableViewSources
{
    public class MessagesSource : UITableViewSource
    {
        private readonly List<Message> _messages;

        private readonly Account _selectedAccount;
        private readonly ExtensionWithCount _selectedExtension;
        private readonly FolderWithCount _selectedFolder;

        private readonly UINavigationController _navigationController;

        private int _selectedRowIndex = -1;
        private int _selectedRowForHeight = -1;

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
            var selectedMessage = _messages[indexPath.Row];

            if (_selectedRowIndex >= 0 && indexPath.Row == _selectedRowIndex)
            {
                _selectedRowIndex = -1;
                var expandedCell = tableView.DequeueReusableCell(RecentCell.RecentCellId) as ExpandedCell ?? new ExpandedCell(selectedMessage.Type);
                expandedCell.UpdateCell(selectedMessage.Name, Formatting.DateTimeFormat(selectedMessage.ReceivedOn), selectedMessage.Length, "sample.m4a");

                return expandedCell;
            }

            var cell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell ?? new MessageCell();
            cell.TextLabel.Text = selectedMessage.Name;
            cell.TextLabel.Font = UIFont.SystemFontOfSize(17, selectedMessage.Unread ? UIFontWeight.Bold : UIFontWeight.Regular);
            cell.DetailTextLabel.Text = Formatting.DateTimeFormat(selectedMessage.ReceivedOn);
            cell.ImageView.Image = Appearance.GetMessageImage(selectedMessage.Type, selectedMessage.Unread, false);
            cell.Accessory = UITableViewCellAccessory.None;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            _selectedRowForHeight = _selectedRowIndex = indexPath.Row;
            tableView.ReloadRows(tableView.IndexPathsForVisibleRows, UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];

            if (_selectedRowForHeight >= 0 && indexPath.Row == _selectedRowForHeight)
                return selectedMessage.Type == MessageType.Fax ? 100 : 138;

            return 48;
        }
    }
}