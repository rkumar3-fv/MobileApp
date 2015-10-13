using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountsTableViewController : UITableViewController
	{
	    public List<Account> Accounts { get; set; }

		public AccountsTableViewController(IntPtr handle) : base (handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            NavigationController.NavigationBar.BarTintColor = new UIColor(0.016f, 0.588f, 0.816f, 1);

            AccountsTableView.Source = new AccountSource(Accounts, NavigationController);
	    }
	}
}