using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public List<Message> Messages;

        private readonly Account _selectedAccount;

        private readonly UINavigationController _navigationController;

        public NSIndexPath SelectedRowIndexPath;
        public NSIndexPath DeletedRowIndexPath;

        private ExpandedCell _expandedCell;

        public MessagesSource(List<Message> messages, Account selectedAccount, UINavigationController navigationController)
        {
            Messages = messages;
            _selectedAccount = selectedAccount;

            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var message = Messages[indexPath.Row];

            if (indexPath.Row == SelectedRowIndexPath?.Row)
            {
                _expandedCell = tableView.DequeueReusableCell(ExpandedCell.ExpandedCellId) as ExpandedCell ?? new ExpandedCell(message) { SelectionStyle = UITableViewCellSelectionStyle.None };

                _expandedCell.UpdateCell(message, _selectedAccount.PhoneNumber);
                ProceedEventsSubscription(tableView, SelectedRowIndexPath);

                return _expandedCell;
            }

            var messageCell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell ?? new MessageCell { Accessory = UITableViewCellAccessory.None };
            messageCell.UpdateCell(message);

            return messageCell;
        }

        private void ProceedEventsSubscription(UITableView tableView, NSIndexPath indexPath)
        {
            if (_expandedCell == null)
                return;

            _expandedCell.OnCallbackClick -= OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick -= OnViewFaxClick(indexPath);
            _expandedCell.OnPlayClick -= OnPlayClick(indexPath);
            _expandedCell.OnDeleteMessageClick -= OnDeleteMessageClick(tableView);
            _expandedCell.OnCallbackClick += OnCallbackClick(indexPath);
            _expandedCell.OnViewFaxClick += OnViewFaxClick(indexPath);
            _expandedCell.OnPlayClick += OnPlayClick(indexPath);
            _expandedCell.OnDeleteMessageClick += OnDeleteMessageClick(tableView);
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

        private EventHandler<ExpandedCellButtonClickEventArgs> OnDeleteMessageClick(UITableView tableView)
        {
            return (sender, args) => RowDeleteClick(tableView, DeletedRowIndexPath);
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var messagesCount = Messages?.Count ?? 0;

            if (tableview.BackgroundView != null)
            {
                tableview.SeparatorStyle = messagesCount == 0 ? UITableViewCellSeparatorStyle.None : UITableViewCellSeparatorStyle.SingleLine;
                tableview.BackgroundView.Hidden = messagesCount != 0;
            }

            return messagesCount;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            if (Equals(SelectedRowIndexPath, indexPath))
                return;

            AppDelegate.ResetAudioPlayer();

            var previousSelectedPath = SelectedRowIndexPath;
            SelectedRowIndexPath = indexPath;
            DeletedRowIndexPath = indexPath;

            var indexes = new List<NSIndexPath>();
            if (previousSelectedPath != null && tableView.CellAt(previousSelectedPath) != null && !Equals(previousSelectedPath, indexPath))
                indexes.Add(previousSelectedPath);

            if (indexPath != null && tableView.CellAt(indexPath) != null)
                indexes.Add(indexPath);

            tableView.ReloadRows(indexes.ToArray(), UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return indexPath.Row == SelectedRowIndexPath?.Row ? (Messages[indexPath.Row].Type == MessageType.Fax ? 100 : 138) : 48;
        }

        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowCallbackClick;
        public event EventHandler<ExpandedCellButtonClickEventArgs> OnRowViewFaxClick;

        private void RowPlayClick(NSIndexPath indexPath)
        {
            if (IndexPathIsInvalid(indexPath)) return;

            Messages[indexPath.Row].Unread = false;
        }

        private void RowViewFaxClick(NSIndexPath indexPath, string filePath)
        {
            if (IndexPathIsInvalid(indexPath)) return;

            Messages[indexPath.Row].Unread = false;
            OnRowViewFaxClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(filePath));
        }

        private void RowCallbackClick(NSIndexPath indexPath)
        {
            if (IndexPathIsInvalid(indexPath)) return;

            OnRowCallbackClick?.Invoke(this, new ExpandedCellButtonClickEventArgs(Messages[indexPath.Row]));
        }

        private void RowDeleteClick(UITableView tableView, NSIndexPath indexPath)
        {
            var alertController = UIAlertController.Create("Confirm Deletion", "Delete this message?", UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Don't delete", UIAlertActionStyle.Cancel, a => ProceedEventsSubscription(tableView, indexPath)));
            alertController.AddAction(UIAlertAction.Create("Delete", UIAlertActionStyle.Default, a => DeleteMessageClick(tableView, indexPath)));

            _navigationController.PresentViewController(alertController, true, null);
        }

        private async void DeleteMessageClick(UITableView tableView, NSIndexPath indexPath)
        {
            if (IndexPathIsInvalid(indexPath)) return;

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            var selectedMessage = Messages[indexPath.Row];

            if (tableView.CellAt(indexPath) is ExpandedCell)
            {
                SelectedRowIndexPath = null;
                tableView.DeselectRow(indexPath, false);
            }

            if (SelectedRowIndexPath != null && SelectedRowIndexPath.Row > indexPath.Row)
            {
                SelectedRowIndexPath = NSIndexPath.FromRowSection(SelectedRowIndexPath.Row - 1, 0);
                DeletedRowIndexPath = SelectedRowIndexPath;
            }

            if (selectedMessage.Id == AppDelegate.ActivePlayerMessageId)
                AppDelegate.ResetAudioPlayer();

            var model = new ExpandedCellViewModel(_selectedAccount.PhoneNumber, selectedMessage.Mailbox, selectedMessage.Id);
            if (selectedMessage.Folder == "Trash")
            {
                var watcher = Stopwatch.StartNew();
                await model.DeleteMessageAsync();
                watcher.Stop();
                Log.ReportTime(Log.EventCategory.Request, "DeleteMessage", "", watcher.ElapsedMilliseconds);
            }
            else
            {
                var watcher = Stopwatch.StartNew();
                await model.MoveMessageToTrashAsync();
                watcher.Stop();
                Log.ReportTime(Log.EventCategory.Request, "MoveMessageToTrash", "", watcher.ElapsedMilliseconds);
            }

            tableView.BeginUpdates();
            Messages.RemoveAt(indexPath.Row);
            tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Left);
            tableView.EndUpdates();
        }

        private bool IndexPathIsInvalid(NSIndexPath indexPath)
        {
            return indexPath == null || indexPath.Row < 0 || Messages.Count == 0 || Messages.Count == indexPath.Row;
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