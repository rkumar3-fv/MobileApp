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

        private NSIndexPath _selectedRowIndexPath;
        private int _selectedRowIndex = -1;

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

            if (indexPath.Row == _selectedRowIndex)
            {
                var expandedCell = tableView.DequeueReusableCell(RecentCell.RecentCellId) as ExpandedCell;
                if (expandedCell != null) return expandedCell;

                expandedCell = new ExpandedCell(selectedMessage);
                expandedCell.UpdateCell(_selectedAccount.PhoneNumber);

                expandedCell.OnCallbackClick += (sender, args) => { RowCallbackClick(tableView, indexPath, args.SourceNumber); };

                return expandedCell;
            }

            var cell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell;
            if (cell != null) return cell;

            cell = new MessageCell { Accessory = UITableViewCellAccessory.None };
            cell.TextLabel.Text = selectedMessage.Name;
            cell.TextLabel.Font = UIFont.SystemFontOfSize(17, selectedMessage.Unread ? UIFontWeight.Bold : UIFontWeight.Regular);
            cell.DetailTextLabel.Text = Formatting.DateTimeFormat(selectedMessage.ReceivedOn);
            cell.ImageView.Image = Appearance.GetMessageImage(selectedMessage.Type, selectedMessage.Unread, false);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var previousSelectedRow = _selectedRowIndexPath;
            _selectedRowIndexPath = indexPath;

            _selectedRowIndex = indexPath.Row;

            var indexes = new List<NSIndexPath>();
            if (previousSelectedRow != null)
                indexes.Add(previousSelectedRow);

            indexes.Add(indexPath);
            tableView.ReloadRows(indexes.ToArray(), UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];

            return indexPath.Row == _selectedRowIndex ? (selectedMessage.Type == MessageType.Fax ? 100 : 138) : 48;
        }

        public event EventHandler<CallBackClickEventArgs> OnRowCallbackClick;

        public class CallBackClickEventArgs : EventArgs
        {
            public UITableView TableView { get; private set; }
            public NSIndexPath IndexPath { get; private set; }
            public string SourceNumber { get; private set; }

            public CallBackClickEventArgs(UITableView tableView, NSIndexPath indexPath, string sourceNumber)
            {
                TableView = tableView;
                IndexPath = indexPath;
                SourceNumber = sourceNumber;
            }
        }

        private void RowCallbackClick(UITableView tableView, NSIndexPath indexPath, string sourceNumber)
        {
            OnRowCallbackClick?.Invoke(this, new CallBackClickEventArgs(tableView, indexPath, sourceNumber));
        }
    }
}