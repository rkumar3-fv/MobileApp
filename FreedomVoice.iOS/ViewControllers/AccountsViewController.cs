using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using GoogleAnalytics.iOS;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountsViewController : BaseViewController
	{
        public List<Account> AccountsList;

	    public AccountsViewController(IntPtr handle) : base(handle)
	    {
            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Accounts Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

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

            base.ViewWillAppear(animated);
        }
    }
}