using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.Entities.Enums;
using FreedomVoice.iOS.TableViewCells.Texting.Messaging;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationSource: UITableViewSource
    {
        public event EventHandler NeedMoreEvent;
        private readonly UITableView _tableView;
        private List<IChatMessage> _messages = new List<IChatMessage>();

        public ConversationSource(UITableView tableView)
        {
            _tableView = tableView;

            tableView.Transform = CGAffineTransform.MakeScale(1, -1);


            tableView.RegisterNibForCellReuse(IncomingMessageTableViewCell.Nib, IncomingMessageTableViewCell.Key);
            tableView.RegisterNibForCellReuse(OutgoingMessageTableViewCell.Nib, OutgoingMessageTableViewCell.Key);
            tableView.RegisterNibForCellReuse(MessageDateTableViewCell.Nib, MessageDateTableViewCell.Key);
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 20;
            tableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = _messages[indexPath.Row];
            switch (item.Type)
            {
                case ChatMessageType.Date:
                    var dateCell = tableView.DequeueReusableCell(MessageDateTableViewCell.Key) as MessageDateTableViewCell;
                    dateCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    dateCell.Date = item.Message;

                    return dateCell;
                case ChatMessageType.Incoming:
                    var incCell = tableView.DequeueReusableCell(IncomingMessageTableViewCell.Key) as IncomingMessageTableViewCell;
                    incCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    incCell.Text = item.Message;
                    incCell.Time = item.Time;
                    return incCell;
                case ChatMessageType.Outgoing:
                    var outCell = tableView.DequeueReusableCell(OutgoingMessageTableViewCell.Key) as OutgoingMessageTableViewCell;
                    outCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    outCell.Text = item.Message;
                    outCell.Time = item.Time;
                    outCell.State = item.SendingState;
                    return outCell;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            if (indexPath.Row >= _messages.Count - 15)
            {
                NeedMoreEvent?.Invoke(this, null);
            }
            var outCell = cell as OutgoingMessageTableViewCell;
            outCell?.VisibilityUpdated();
        }


        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _messages.Count;
        }

        public void UpdateItems(List<IChatMessage> Messages)
        {
            _messages = Messages;
            _tableView.ReloadData();
        }
    }
}
