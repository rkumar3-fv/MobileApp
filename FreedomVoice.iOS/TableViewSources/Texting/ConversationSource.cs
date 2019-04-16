using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.TableViewCells.Texting.Messaging;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationSource: UITableViewSource
    {
        private readonly UITableView _tableView;

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
            var cell = tableView.DequeueReusableCell("DateCell") as MessageDateTableViewCell;
            cell.ContentView.Transform = CGAffineTransform.MakeScale(1, -1);
            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return 55;
        }
    }
}
