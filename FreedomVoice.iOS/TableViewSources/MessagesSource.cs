using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using UIKit;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
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
        private NSIndexPath _deletedRowIndexPath;

        private ExpandedCell _expandedCell;

        public MessagesSource(List<Message> messages, Account selectedAccount, UINavigationController navigationController)
        {
            _messages = messages;
            _selectedAccount = selectedAccount;

            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];

            if (indexPath.Row == _selectedRowIndexPath?.Row)
            {
                _expandedCell = tableView.DequeueReusableCell(ExpandedCell.ExpandedCellId) as ExpandedCell ?? new ExpandedCell(selectedMessage, _navigationController);

                _expandedCell.UpdateCell(selectedMessage, _selectedAccount.PhoneNumber);
                ProceedEventsSubscription(tableView, _selectedRowIndexPath);

                return _expandedCell;
            }

            var cell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell ?? new MessageCell { Accessory = UITableViewCellAccessory.None };

            cell.TextLabel.Text = !string.IsNullOrEmpty(selectedMessage.SourceName.Trim()) ? selectedMessage.SourceName.Trim() : (!string.IsNullOrEmpty(selectedMessage.SourceNumber.Trim()) ? DataFormatUtils.ToPhoneNumber(selectedMessage.SourceNumber.Trim()) : "Unavailable");
            cell.TextLabel.Font = UIFont.SystemFontOfSize(17, selectedMessage.Unread ? UIFontWeight.Bold : UIFontWeight.Regular);
            cell.DetailTextLabel.Text = DataFormatUtils.ToFormattedDate("Yesterday", selectedMessage.ReceivedOn);
            cell.ImageView.Image = Appearance.GetMessageImage(selectedMessage.Type, selectedMessage.Unread, false);

            return cell;
        }

        private void ProceedEventsSubscription(UITableView tableView, NSIndexPath indexPath)
        {
            if (_expandedCell == null)
                return;

            _expandedCell.OnCallbackClick -= OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick -= OnViewFaxClick();
            _expandedCell.DeleteButton.TouchDown -= OnDeleteMessageClick(tableView);
            _expandedCell.OnCallbackClick += OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick += OnViewFaxClick();
            _expandedCell.DeleteButton.TouchDown += OnDeleteMessageClick(tableView);
        }

        private EventHandler<ExpandedCellButtonClickEventArgs> OnViewFaxClick()
        {
            return (sender, args) => RowViewFaxClick(args.FilePath);
        }

        private EventHandler<ExpandedCellButtonClickEventArgs> OnCallbackClick(NSIndexPath indexPath)
        {
            return (sender, args) => RowCallbackClick(indexPath);
        }

        private EventHandler OnDeleteMessageClick(UITableView tableView)
        {
            return (sender, args) =>
            {
                var alertController = UIAlertController.Create("Confirm deletion", "Delete this message?", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Default, a => {
                    DeleteMessageClick(tableView, _deletedRowIndexPath);
                }));
                alertController.AddAction(UIAlertAction.Create("Don't delete", UIAlertActionStyle.Cancel, a => { }));

                _navigationController.PresentViewController(alertController, true, null);
            };
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (Equals(_selectedRowIndexPath, indexPath))
                return;

            _deletedRowIndexPath = indexPath;

            var previousSelectedPath = _selectedRowIndexPath;
            _selectedRowIndexPath = indexPath;

            var indexes = new List<NSIndexPath>();
            if (previousSelectedPath != null && tableView.CellAt(previousSelectedPath) != null && !Equals(previousSelectedPath, indexPath))
                indexes.Add(previousSelectedPath);

            if (indexPath != null && tableView.CellAt(indexPath) != null)
                indexes.Add(indexPath);

            tableView.ReloadRows(indexes.ToArray(), UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return indexPath.Row == _selectedRowIndexPath?.Row ? (_messages[indexPath.Row].Type == MessageType.Fax ? 100 : 138) : 48;
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowViewFaxClick;

        private void RowViewFaxClick(string filePath)
        {
            OnRowViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void RowCallbackClick(NSIndexPath indexPath)
        {
            OnRowCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(_messages[indexPath.Row]));
        }

        private async void DeleteMessageClick(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];

            if (tableView.CellAt(indexPath) is ExpandedCell)
            {
                _selectedRowIndexPath = null;
                tableView.DeselectRow(indexPath, false);
            }

            if (_selectedRowIndexPath != null && _selectedRowIndexPath.Row > indexPath.Row)
            {
                _selectedRowIndexPath = NSIndexPath.FromRowSection(_selectedRowIndexPath.Row - 1, 0);
                _deletedRowIndexPath = _selectedRowIndexPath;
            }

            var model = new ExpandedCellViewModel(_selectedAccount.PhoneNumber, selectedMessage.Mailbox, selectedMessage.Id, _navigationController);
            if (selectedMessage.Folder == "Trash")
                await model.DeleteMessageAsync();
            else
                await model.MoveMessageToTrashAsync();

            tableView.BeginUpdates();
            _messages.RemoveAt(indexPath.Row);
            tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Fade);
            tableView.EndUpdates();
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    DeleteMessageClick(tableView, indexPath);
                    break;                    
            }
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }
    }
}