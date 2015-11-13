using System;
using System.Collections.Generic;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class RecentsSource : UITableViewSource
    {
        private List<Recent> _recents;        

        public RecentsSource(List<Recent> recents)
        {
            _recents = recents;            
        }

        public void SetRecents(List<Recent> recents)
        {
            _recents = recents;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var recent = _recents[indexPath.Row];
            
            var cell = tableView.DequeueReusableCell(RecentCell.RecentCellId) as RecentCell ?? new RecentCell();

            var dialDateLabelText = recent.FormatedDialDate;
            var dialDateLabelWidth = ((NSString)dialDateLabelText).StringSize(UIFont.SystemFontOfSize(12)).Width;
            var dialDatePositionX = Theme.ScreenBounds.Width - dialDateLabelWidth - 45;

            cell.DialDate.Frame = new CGRect(dialDatePositionX, 14, dialDateLabelWidth, 16);
            cell.PhoneTitle.Frame = new CGRect(15, 11, dialDatePositionX - 20, 22);

            cell.PhoneTitle.Text = recent.TitleOrNumber;
            cell.DialDate.Text = dialDateLabelText;

            cell.Accessory = string.IsNullOrEmpty(recent.Title) ? UITableViewCellAccessory.None : UITableViewCellAccessory.DetailButton;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _recents.Count;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 44;
        }

        public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
        {
            var recent = _recents[indexPath.Row];
            OnRecentInfoClicked?.Invoke(recent);
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:                    
                    RowDeleted(tableView, indexPath);                                        
                    break;                    
                case UITableViewCellEditingStyle.None:
                case UITableViewCellEditingStyle.Insert:
                    break;
            }
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public Action<Recent> OnRecentInfoClicked;

        public event EventHandler<RowSelectedEventArgs> OnRowSelected;
        public event EventHandler<RowSelectedEventArgs> OnRowDeleted;

        public class RowSelectedEventArgs : EventArgs
        {
            public UITableView TableView { get; private set; }
            public NSIndexPath IndexPath { get; private set; }

            public RowSelectedEventArgs(UITableView tableView, NSIndexPath indexPath)
            {
                TableView = tableView;
                IndexPath = indexPath;
            }
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            OnRowSelected?.Invoke(this, new RowSelectedEventArgs(tableView, indexPath));
        }

        private void RowDeleted(UITableView tableView, NSIndexPath indexPath)
        {
            OnRowDeleted?.Invoke(this, new RowSelectedEventArgs(tableView, indexPath));
        }
    }
}