using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Foundation;
using FreedomVoice.Core.Presenters;
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
        private readonly ConversationsPresenter _presenter;
        private readonly UINavigationController _navigationController;
        private readonly UITableView _tableView;

        public ConversationsSource(ConversationsPresenter presenter, UINavigationController navigationController, UITableView tableView)
        {
            _presenter = presenter;
            _presenter.ItemsChanged += (sender, e) =>
            {
                tableView.ReloadData();
            };

            _tableView = tableView;

            _navigationController = navigationController;

            tableView.RegisterNibForCellReuse(UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle), "cell");
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = 40;
            _presenter.ReloadAsync();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cell") as ConversationItemTableViewCell;
            var item = _presenter.Items[indexPath.Row];

            Debug.Assert(cell != null, nameof(cell) + " != null");
            cell.Title = item.Collocutor;
            cell.Detail = item.LastMessage;
            cell.Date = item.Date;
            cell.isNew = item.IsNew;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _presenter.Items.Count;
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
        private void ScrollViewDidScroll(UIScrollView scrollView)
        {
            if (scrollView.ContentOffset.Y >= scrollView.ContentSize.Height - 200 && _presenter.HasMore)
            {
                _presenter.LoadMore();
            }
        }

     
    }
}