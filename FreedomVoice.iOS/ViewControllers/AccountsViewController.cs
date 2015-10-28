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
            Title = "Select Account";

            AccountsTableView.TableFooterView = new UIKit.UIView(CoreGraphics.CGRect.Empty);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            AccountsTableView.Source = new AccountSource(AccountsList, NavigationController);

            base.ViewDidLoad();
        }
    }
}