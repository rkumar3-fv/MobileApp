using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS.TableViewSources
{
    public class AccountSource : UITableViewSource
    {
        private readonly List<Account> _accounts;
        private readonly UINavigationController _navigationController;

        public AccountSource(List<Account> accounts, UINavigationController navigationController)
        {
            _accounts = accounts;
            _navigationController = navigationController;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(AccountCell.Key) as AccountCell ?? new AccountCell();
            cell.TextLabel.Text = _accounts[indexPath.Row].FormattedPhoneNumber;
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _accounts.Count;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            var selectedAccount = _accounts[indexPath.Row];

            //TODO: Account not available
            //if (AccountNotAvailable)
            //{
            //    var accountUnavailableController = AppDelegate.GetViewController<AccountUnavailableViewController>("AccountUnavailableViewController");
            //    accountUnavailableController.SelectedAccount = selectedAccount;
            //    accountUnavailableController.Title = selectedAccount.FormattedPhoneNumber;
            //    accountUnavailableController.NavigationItem.SetLeftBarButtonItem(Misc.GetBackBarButton(_navigationController), true);

            //    _navigationController.PushViewController(accountUnavailableController, true);

            //    return;
            //}

            if (!AppDelegate.DisclaimerWasShown)
            {
                var emergencyDisclaimerController = AppDelegate.GetViewController<EmergencyDisclaimerViewController>("EmergencyDisclaimerViewController");
                emergencyDisclaimerController.SelectedAccount = selectedAccount;
                emergencyDisclaimerController.ViewController = _navigationController;

                var rootNavigationController = new UINavigationController();
                rootNavigationController.PushViewController(emergencyDisclaimerController, false);

                (UIApplication.SharedApplication.Delegate as AppDelegate).SetRootViewController(rootNavigationController, true);

                return;
            }

            var tabBarController = AppDelegate.GetViewController<MainTabBarController>("MainTabBarController");
            tabBarController.SelectedAccount = selectedAccount;
            tabBarController.Title = selectedAccount.FormattedPhoneNumber;
            tabBarController.NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(_navigationController), true);

            _navigationController.PushViewController(tabBarController, true);
        }
    }
}