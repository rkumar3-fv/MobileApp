using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountsViewController : BaseViewController
	{
        public List<Account> AccountsList;

        public AccountsViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            AccountsTableView.TableFooterView = new UIKit.UIView(CoreGraphics.CGRect.Empty);

            AccountsTableView.Source = new AccountSource(AccountsList, NavigationController);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = "Select Account";

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);
            NavigationController.NavigationBarHidden = false;

            base.ViewWillAppear(animated);
        }
    }
}