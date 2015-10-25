using System;
using System.Collections.Generic;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewCells;
using FreedomVoice.iOS.Utilities;
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
            var cell = tableView.DequeueReusableCell(AccountCell.AccountCellId) as AccountCell ?? new AccountCell();
            cell.TextLabel.Text = _accounts[indexPath.Row].FormattedPhoneNumber;
            cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;

            return cell;
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return _accounts?.Count ?? 0;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedAccount = _accounts[indexPath.Row];

            if (!UserDefault.DisclaimerWasShown)
            {
                var emergencyDisclaimerController = AppDelegate.GetViewController<EmergencyDisclaimerViewController>();
                emergencyDisclaimerController.SelectedAccount = selectedAccount;
                emergencyDisclaimerController.ParentController = _navigationController;

                var navigationController = new UINavigationController(emergencyDisclaimerController);
                Theme.TransitionController(navigationController);

                return;
            }

            var tabBarController = AppDelegate.GetViewController<MainTabBarController>();
            tabBarController.SelectedAccount = selectedAccount;

            _navigationController.PushViewController(tabBarController, true);
        }
    }
}