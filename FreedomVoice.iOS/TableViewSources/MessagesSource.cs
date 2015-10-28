using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;
using FreedomVoice.Core.Entities.Enums;

namespace FreedomVoice.iOS.TableViewSources
{
    public class MessagesSource : UITableViewSource
    {
        private readonly List<Message> _messages;

        private readonly Account _selectedAccount;
        private readonly ExtensionWithCount _selectedExtension;
        private readonly FolderWithCount _selectedFolder;

        private readonly UINavigationController _navigationController;
        private int selectedRowIndex = -1;
        private int selectedRowForHeight = -1;

        public MessagesSource(List<Message> messages, ExtensionWithCount selectedExtension, Account selectedAccount, FolderWithCount folder, UINavigationController navigationController)
        {
            _messages = messages;

            _selectedExtension = selectedExtension;
            _selectedAccount = selectedAccount;
            _selectedFolder = folder;

            _navigationController = navigationController;
        }

        MessageType GetRandomType()
        {
            Array values = Enum.GetValues(typeof(MessageType));
            Random random = new Random();
            return (MessageType)values.GetValue(random.Next(values.Length));
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {

            var dateFormatter = new NSDateFormatter
            {
                DoesRelativeDateFormatting = true,
                DateStyle = NSDateFormatterStyle.Short,
                TimeStyle = NSDateFormatterStyle.Short
            };

            if (selectedRowIndex >= 0 && indexPath.Row == selectedRowIndex)
            {
                var selectedMessage = _messages[indexPath.Row];
                selectedMessage.Type = GetRandomType();
                selectedRowIndex = -1;
                var expandedCell = tableView.DequeueReusableCell(RecentCell.RecentCellId) as ExpandedCell ?? new ExpandedCell(selectedMessage.Type);
                expandedCell.UpdateCell(selectedMessage.Name, dateFormatter.ToString(selectedMessage.ReceivedOn), selectedMessage.Length, "sample.m4a");
                return expandedCell;
            }

            var folder = _messages[indexPath.Row];
            

            var cell = tableView.DequeueReusableCell(MessageCell.MessageCellId) as MessageCell ?? new MessageCell();
            cell.TextLabel.Text = folder.Name;
            cell.DetailTextLabel.Text = dateFormatter.ToString(folder.ReceivedOn);
            //cell.ImageView.Image = GetImageByFolderName(_messages[indexPath.Row].Name);
            cell.Accessory = UITableViewCellAccessory.None;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            selectedRowForHeight = selectedRowIndex = indexPath.Row;
            var selectedMessage = _messages[indexPath.Row];
            tableView.ReloadRows(tableView.IndexPathsForVisibleRows, UITableViewRowAnimation.Fade);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedMessage = _messages[indexPath.Row];

            if (selectedRowForHeight >= 0 && indexPath.Row == selectedRowForHeight)
                return selectedMessage.Type == MessageType.Fax ? 100 : 138;

            return 40;
        }
    }
}