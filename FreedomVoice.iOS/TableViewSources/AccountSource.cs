using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class AccountSource : UITableViewSource
    {
        private readonly List<Account> _accounts;
        private readonly UINavigationController _viewController;

        public AccountSource(List<Account> accounts, UINavigationController viewController)
        {
            _accounts = accounts;
            _viewController = viewController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(AccountCell.Key) as AccountCell ?? new AccountCell();
            cell.TextLabel.Text = _accounts[indexPath.Row].FormattedPhoneNumber;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _accounts.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;

            var tabBarController = appDelegate.GetViewController(appDelegate.MainStoryboard, "MainTabBarController") as MainTabBarController;
            var selectedAccount = _accounts[indexPath.Row];
            tabBarController.SelectedAccount = selectedAccount;
            tabBarController.Title = selectedAccount.FormattedPhoneNumber;

            _viewController.PushViewController(tabBarController, true);
        }
    }
}