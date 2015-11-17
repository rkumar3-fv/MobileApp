using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class MessagesSource : UITableViewSource
    {
        private readonly List<Message> _messagesList;

        private readonly Account _selectedAccount;

        private readonly UINavigationController _navigationController;

        private NSIndexPath _selectedRowIndexPath;
        private NSIndexPath _deletedRowIndexPath;

        private ExpandedCell _expandedCell;

        public MessagesSource(List<Message> messages, Account selectedAccount, UINavigationController navigationController)
        {
            _messagesList = messages;
            _selectedAccount = selectedAccount;

            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messagesList[indexPath.Row];

            if (indexPath.Row == _selectedRowIndexPath?.Row)
            {
                _expandedCell = tableView.DequeueReusableCell(ExpandedCell.ExpandedCellId) as ExpandedCell ?? new ExpandedCell(selectedMessage, _navigationController);

                var activePlayer = selectedMessage.Type != MessageType.Fax &&
                                   selectedMessage.Id == AppDelegate.ActivePlayerMessageId
                    ? AppDelegate.ActivePlayerView
                    : null;

                _expandedCell.UpdateCell(selectedMessage, _selectedAccount.PhoneNumber, activePlayer);
                ProceedEventsSubscription(tableView, _selectedRowIndexPath);

                return _expandedCell;
            }

            var messageCell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell ?? new MessageCell { Accessory = UITableViewCellAccessory.None };
            messageCell.UpdateCell(selectedMessage);

            return messageCell;
        }

        private void ProceedEventsSubscription(UITableView tableView, NSIndexPath indexPath)
        {
            if (_expandedCell == null)
                return;

            _expandedCell.OnCallbackClick -= OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick -= OnViewFaxClick(indexPath);
            _expandedCell.OnPlayClick -= OnPlayClick(indexPath);
            _expandedCell.DeleteButton.TouchDown -= OnDeleteMessageClick(tableView);
            _expandedCell.OnCallbackClick += OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick += OnViewFaxClick(indexPath);
            _expandedCell.OnPlayClick += OnPlayClick(indexPath);
            _expandedCell.DeleteButton.TouchDown += OnDeleteMessageClick(tableView);
        }

        private EventHandler<ExpandedCellButtonClickEventArgs> OnViewFaxClick(NSIndexPath indexPath)
        {
            return (sender, args) => RowViewFaxClick(indexPath, args.FilePath);
        }

        private EventHandler<ExpandedCellButtonClickEventArgs> OnPlayClick(NSIndexPath indexPath)
        {
            return (sender, args) => RowPlayClick(indexPath);
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
            return _messagesList?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (Equals(_selectedRowIndexPath, indexPath))
                return;

            var previousSelectedPath = _selectedRowIndexPath;
            _selectedRowIndexPath = indexPath;
            _deletedRowIndexPath = indexPath;

            var indexes = new List<NSIndexPath>();
            if (previousSelectedPath != null && tableView.CellAt(previousSelectedPath) != null && !Equals(previousSelectedPath, indexPath))
                indexes.Add(previousSelectedPath);

            if (indexPath != null && tableView.CellAt(indexPath) != null)
                indexes.Add(indexPath);

            tableView.ReloadRows(indexes.ToArray(), UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return indexPath.Row == _selectedRowIndexPath?.Row ? (_messagesList[indexPath.Row].Type == MessageType.Fax ? 100 : 138) : 48;
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowViewFaxClick;

        private void RowPlayClick(NSIndexPath indexPath)
        {
            _messagesList[indexPath.Row].Unread = false;
        }

        private void RowViewFaxClick(NSIndexPath indexPath, string filePath)
        {
            _messagesList[indexPath.Row].Unread = false;
            OnRowViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void RowCallbackClick(NSIndexPath indexPath)
        {
            OnRowCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(_messagesList[indexPath.Row]));
        }

        private async void DeleteMessageClick(UITableView tableView, NSIndexPath indexPath)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(_navigationController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            var selectedMessage = _messagesList[indexPath.Row];

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
            _messagesList.RemoveAt(indexPath.Row);
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