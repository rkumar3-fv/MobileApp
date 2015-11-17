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

        public async override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            tableView.DeselectRow(indexPath, false);
            var selectedAccount = _accounts[indexPath.Row];

            UserDefault.LastUsedAccount = selectedAccount.PhoneNumber;

            if (!UserDefault.IsLaunchedBefore)
            {
                var phoneNumberController = AppDelegate.GetViewController<PhoneNumberViewController>();
                phoneNumberController.SelectedAccount = selectedAccount;
                phoneNumberController.ParentController = _navigationController;

                var navigationController = new UINavigationController(phoneNumberController);
                Theme.TransitionController(navigationController);

                return;
            }

            var mainTabBarController = await AppDelegate.GetMainTabBarController(selectedAccount, _navigationController);
            if (mainTabBarController != null)
                _navigationController.PushViewController(mainTabBarController, false);
        }
    }
}