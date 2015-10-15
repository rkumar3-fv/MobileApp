using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { get; set; }

	    public bool ShowBackButton { get; set; }

        public MainTabBarController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

	        SelectedIndex = 2;

            if (ShowBackButton)
                NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(NavigationController, "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);
            NavigationController.SetDefaultNavigationBarStyle();
	    }
	}
}