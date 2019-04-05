using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.TableViewCells.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationsSource : UITableViewSource
    {
        private readonly ConversationsViewModel _viewModel;
        private readonly UINavigationController _navigationController;

        public ConversationsSource(ConversationsViewModel viewModel, UINavigationController navigationController, UITableView tableView)
        {
            _viewModel = viewModel;
            _viewModel.ItemsChanged += (object sender, EventArgs e) =>
            {
                tableView.ReloadData();
            };

            _navigationController = navigationController;
            
            tableView.RegisterNibForCellReuse(UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle), "cell" );
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 40;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cell") as ConversationItemTableViewCell;
            var item = _viewModel.Items[indexPath.Row];
            cell.Title = item.CollocutorPhone.PhoneNumber;
            var message = item.Messages.First();
            cell.Detail = message.Text;
            cell.Date = message.To.Equals(_viewModel.PhoneNumber) ? message.ReceivedAt.ToString() : message.SentAt.ToString();

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _viewModel.Items.Count();
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return UITableView.AutomaticDimension;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, true);
        }
        
        [Export("scrollViewDidScroll:")]
        private void ScrollViewDidScroll(UIScrollView scrollView) {
            if (scrollView.ContentOffset.Y >= scrollView.ContentSize.Height - 200 && _viewModel.HasMore)
            {
                _viewModel.LoadMore();
            }
        }

    }
}