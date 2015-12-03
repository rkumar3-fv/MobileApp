using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountsViewController : BaseViewController
	{
        protected override string PageName => "Accounts Screen";

        public List<Account> AccountsList;

	    public AccountsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            var accountsTableView = new UITableView
            {
                Frame = Theme.ScreenBounds,
                TableFooterView = new UIView(CoreGraphics.CGRect.Empty),
                Source = new AccountSource(AccountsList, NavigationController)
            };

            View.Add(accountsTableView);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = "Select Account";

            UserDefault.LastUsedAccount = string.Empty;

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);
            NavigationController.NavigationBarHidden = false;

            AppDelegate.ActivityIndicator.SetActivityIndicatorCenter(Theme.ScreenCenter);

            base.ViewWillAppear(animated);
        }
    }
}