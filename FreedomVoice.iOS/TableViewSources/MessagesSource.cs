using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using UIKit;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.ViewModels;

namespace FreedomVoice.iOS.TableViewSources
{
    public class MessagesSource : UITableViewSource
    {
        private readonly List<Message> _messages;

        private readonly Account _selectedAccount;

        private readonly UINavigationController _navigationController;

        private NSIndexPath _selectedRowIndexPath;
        private int _selectedRowIndex = -1;
        private int _selectedRowIndexForHeight = -1;

        public MessagesSource(List<Message> messages, Account selectedAccount, UINavigationController navigationController)
        {
            _messages = messages;
            _selectedAccount = selectedAccount;

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
                expandedCell.UpdateCell(_selectedAccount.PhoneNumber, _navigationController);

                expandedCell.OnCallbackClick += (sender, args) => { RowCallbackClick(tableView, indexPath); };
                expandedCell.OnViewFaxClick += (sender, args) => { RowViewFaxClick(args.FilePath); };
                expandedCell.OnRowDeleteMessageClick += (sender, args) => { RowDeleteMessageClick(tableView, indexPath); };

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

            _selectedRowIndex = _selectedRowIndexForHeight = indexPath.Row;

            var indexes = new List<NSIndexPath>();
            if (previousSelectedRow != null && tableView.CellAt(previousSelectedRow) != null)
                indexes.Add(previousSelectedRow);

            indexes.Add(indexPath);
            tableView.ReloadRows(indexes.ToArray(), UITableViewRowAnimation.Fade);            
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];
            return indexPath.Row == _selectedRowIndexForHeight ? (selectedMessage.Type == MessageType.Fax ? 100 : 138) : 48;
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowViewFaxClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowDeleteMessageClick;

        private void RowViewFaxClick(string filePath)
        {
            OnRowViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void RowCallbackClick(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];
            OnRowCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(selectedMessage));
        }

        private async void RowDeleteMessageClick(UITableView tableView, NSIndexPath indexPath)
        {            
            if (tableView.CellAt(indexPath) is ExpandedCell)
            {                
                _selectedRowIndex = _selectedRowIndexForHeight = -1;
                _selectedRowIndexPath = null;
            }
            var selectedMessage = _messages[indexPath.Row];
            if (selectedMessage == null) return;

            var model = new ExpandedCellViewModel(_selectedAccount.PhoneNumber, selectedMessage.Mailbox, selectedMessage.Id, _navigationController);
            if (selectedMessage.Folder == "Trash")
                await model.DeleteMessageAsync();
            else
                await model.MoveMessageToTrashAsync();

            _messages.Remove(selectedMessage);
            OnRowDeleteMessageClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(tableView, indexPath));
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:                    
                    RowDeleteMessageClick(tableView, indexPath);                    
                    break;                    
            }
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }
    }
}