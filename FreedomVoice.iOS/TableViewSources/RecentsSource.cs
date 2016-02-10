using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class RecentsSource : UITableViewSource
    {
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        private List<Recent> _recents;
        private readonly UIViewController _viewController;

        public RecentsSource(List<Recent> recents, UIViewController viewController)
        {
            _recents = recents;
            _viewController = viewController;
        }

        public void SetRecents(List<Recent> recents)
        {
            _recents = recents;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var recent = _recents[indexPath.Row];
            
            var cell = tableView.DequeueReusableCell(RecentCell.RecentCellId) as RecentCell ?? new RecentCell();
            cell.UpdateCell(recent);

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var recentsCount = _recents.Count;

            tableview.SeparatorStyle = recentsCount == 0 ? UITableViewCellSeparatorStyle.None : UITableViewCellSeparatorStyle.SingleLine;
            tableview.BackgroundView.Hidden = recentsCount != 0;

            return recentsCount;
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

        public override async void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);

            var recent = Recents.GetRecentsOrdered()[indexPath.Row];
            if (recent == null) return;

            Action onSuccessCallReservation = () => OnSuccessCallReservation(tableView, indexPath, recent);

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, recent.PhoneNumber, _viewController, onSuccessCallReservation);
        }

        private void OnSuccessCallReservation(UITableView tableView, NSIndexPath indexPath, Recent recent)
        {
            Recents.AddRecent(recent.PhoneNumber);
            SetRecents(Recents.GetRecentsOrdered());

            tableView.BeginUpdates();

            tableView.MoveRow(indexPath, NSIndexPath.FromRowSection(0, 0));
            tableView.ReloadData();

            tableView.EndUpdates();
        }

        private void RowDeleted(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);

            var recent = Recents.GetRecentsOrdered()[indexPath.Row];
            if (recent == null) return;

            Recents.RemoveRecent(recent);
            SetRecents(Recents.GetRecentsOrdered());

            tableView.BeginUpdates();

            tableView.DeleteRows(new[] { indexPath }, UITableViewRowAnimation.Left);

            tableView.EndUpdates();
        }
    }
}