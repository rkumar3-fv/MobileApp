﻿using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using UIKit;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;

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
            cell.UpdateCell(recent.TitleOrNumber, recent.FormatedDialDate/*, recent.HasIcon*/);
            if (!string.IsNullOrEmpty(recent.Title))
                cell.Accessory = UITableViewCellAccessory.DetailButton;
            else
                cell.Accessory = UITableViewCellAccessory.None;
            return cell;
        }
        

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _recents.Count;
        }

        public override void AccessoryButtonTapped(UITableView tableView, NSIndexPath indexPath)
        {
            
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:                    
                    _recents.RemoveAt(indexPath.Row);                    
                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    break;                    
                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return true;
        }

        public event EventHandler<RowSelectedEventArgs> OnRowSelected;
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
    }
}
