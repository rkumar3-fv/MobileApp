using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountsTableViewController : UITableViewController
	{
        private LoadingOverlay _loadingOverlay;
	    private List<Account> _accountsList = new List<Account>();

	    public AccountsTableViewController(IntPtr handle) : base(handle) { }

	    public override async void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);
            NavigationController.SetDefaultNavigationBarStyle();

            ShowOverlay();
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            await GetAccountsList();

            if (_accountsList.Count == 1)
            {
                var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
                appDelegate.RootNavigationController = new UINavigationController();

                var tabBarController = AppDelegate.GetViewController<MainTabBarController>("MainTabBarController");
                tabBarController.Title = _accountsList.First().FormattedPhoneNumber;
                tabBarController.SelectedAccount = _accountsList.First();
                appDelegate.RootNavigationController.PushViewController(tabBarController, false);
                appDelegate.SetRootViewController(appDelegate.RootNavigationController, false);
            }

            HideOverlay();
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

	    public override void ViewWillAppear(bool animated)
	    {
            base.ViewWillAppear(animated);

            AccountsTableView.Source = new AccountSource(_accountsList, NavigationController);
            AccountsTableView.ReloadData();
        }

	    private void ShowOverlay()
        {
            _loadingOverlay = new LoadingOverlay(UIScreen.MainScreen.Bounds);
            View.Add(_loadingOverlay);
        }

        private void HideOverlay()
        {
            _loadingOverlay.Hide();
        }

        private async Task GetAccountsList()
        {
            await Task.Run(async () =>
            {
                var systems = await ApiHelper.GetSystems();
                _accountsList = systems.Result.PhoneNumbers.Select(phoneNumber => new Account { PhoneNumber = phoneNumber }).ToList();
            });
        }
    }
}