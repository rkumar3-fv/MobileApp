using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { private get; set; }

        private bool IsRootController => NavigationController.ViewControllers.Length > 1;

        public MainTabBarController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

	        Title = SelectedAccount.FormattedPhoneNumber;
	        SelectedIndex = 2;

            if (IsRootController)
                NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(NavigationController, "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);
	    }
	}
}