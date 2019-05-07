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
using FreedomVoice.iOS.ViewControllers.Texts;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources.Texting
{
    public class ConversationSelectEventArgs : EventArgs
    {
        public long ConversationId;
        public string Name;

        public ConversationSelectEventArgs(long conversationId, string name)
        {
            ConversationId = conversationId;
            Name = name;
        }
    }

    public class ConversationsSource : UITableViewSource
    {
        private struct Appearance
        {
            public static readonly nfloat EstimatedRowHeight = 40;
        }
        
        //The number of items to the end of the list, after which the next page starts loading.
        private const int StartLoadingMoreOffset = 15;
        
        private readonly ConversationsPresenter _presenter;
        private readonly UITableView _tableView;
        public event EventHandler ItemDidSelected;

        public ConversationsSource(ConversationsPresenter presenter, UITableView tableView)
        {
            _presenter = presenter;
            _presenter.ItemsChanged += (sender, e) =>
            {
                tableView.ReloadData();
            };

            _tableView = tableView;

            tableView.RegisterNibForCellReuse(UINib.FromName("ConversationItemTableViewCell", NSBundle.MainBundle), "cell");
            tableView.RowHeight = UITableView.AutomaticDimension;
            tableView.EstimatedRowHeight = Appearance.EstimatedRowHeight;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell("cell") as ConversationItemTableViewCell;
            Debug.Assert(cell != null, nameof(cell) + " != null");

            if (indexPath.Row < _presenter.Items.Count)
            {
                var item = _presenter.Items[indexPath.Row];

                cell.Title = item.Collocutor;
                cell.Detail = item.LastMessage;
                cell.Date = item.Date;
                cell.isNew = item.IsNew;
            }

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
            if (indexPath.Row >= _presenter.Items.Count) return;
            var item = _presenter.Items[indexPath.Row];
            ItemDidSelected?.Invoke(this, new ConversationSelectEventArgs(item.ConversationId, item.Collocutor));
        }

        public override void WillDisplay(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            if (indexPath.Row >= _presenter.Items.Count - StartLoadingMoreOffset && _presenter.HasMore)
            {
                _presenter.LoadMoreAsync();
            }
        }
    }
}