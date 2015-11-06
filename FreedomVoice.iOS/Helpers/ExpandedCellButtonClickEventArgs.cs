using Foundation;
using FreedomVoice.iOS.Entities;
using System;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public class ExpandedCellButtonClickEventArgs : EventArgs
    {

        public Message SelectedMessage { get; private set; }
        public string FilePath { get; set; } = string.Empty;

        public UITableView TableView;
        public NSIndexPath IndexPath;

        public ExpandedCellButtonClickEventArgs(Message selectedMessage)
        {
            SelectedMessage = selectedMessage;
        }

        public ExpandedCellButtonClickEventArgs()
        {

        }

        public ExpandedCellButtonClickEventArgs(UITableView tableView, NSIndexPath indexPath)
        {
            TableView = tableView;
            IndexPath = indexPath;
        }

        public ExpandedCellButtonClickEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }
}
