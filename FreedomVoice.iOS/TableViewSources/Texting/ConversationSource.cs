using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.TableViewCells.Texting.Messaging;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationSource: UITableViewSource
    {
        private readonly UITableView _tableView;
        private List<IChatMessage> _messages = new List<IChatMessage>();

        public ConversationSource(UITableView tableView)
        {
            _tableView = tableView;

            tableView.Transform = CGAffineTransform.MakeScale(1, -1);
            var tmp = tableView.AdjustedContentInset;

            tableView.RegisterNibForCellReuse(UINib.FromName("IncomingMessageTableViewCell", NSBundle.MainBundle), "IncomingCell");
            tableView.RegisterNibForCellReuse(UINib.FromName("OutgoingMessageTableViewCell", NSBundle.MainBundle), "OutgoingCell");
            tableView.RegisterNibForCellReuse(UINib.FromName("MessageDateTableViewCell", NSBundle.MainBundle), "DateCell");
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
                    var dateCell = tableView.DequeueReusableCell("DateCell") as MessageDateTableViewCell;
                    dateCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    dateCell.Date = item.ToString();
                    return dateCell;
                case ChatMessageType.Incoming:
                    var incCell =
                        tableView.DequeueReusableCell("IncomingMessageTableViewCell") as IncomingMessageTableViewCell;
                    incCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    incCell.Text = item.ToString();
                    return incCell;
                case ChatMessageType.Outgoing:
                    var outCell =
                        tableView.DequeueReusableCell("OutgoingMessageTableViewCell") as OutgoingMessageTableViewCell;
                    outCell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
                    outCell.Text = item.ToString();
                    return outCell;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
