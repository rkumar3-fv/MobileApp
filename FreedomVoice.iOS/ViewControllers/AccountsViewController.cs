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
            base.ViewDidLoad();

            Title = "Select Account";
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);

            AccountsTableView.Source = new AccountSource(AccountsList, NavigationController);
        }
    }
}