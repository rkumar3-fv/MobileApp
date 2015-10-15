using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class EmergencyDisclaimerViewController : UIViewController
	{
	    public Account SelectedAccount { private get; set; }
        public UINavigationController ViewController { private get; set; }

        public EmergencyDisclaimerViewController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);

            NavigationController.SetDefaultNavigationBarStyle();

            UnderstandButton.Layer.CornerRadius = 5;
            UnderstandButton.ClipsToBounds = true;
        }

	    partial void UnderstandButton_TouchUpInside(UIButton sender)
	    {
	        AppDelegate.DisclaimerWasShown = true;

            var tabBarController = AppDelegate.GetViewController<MainTabBarController>("MainTabBarController");
            tabBarController.SelectedAccount = SelectedAccount;
            tabBarController.Title = SelectedAccount.FormattedPhoneNumber;
            tabBarController.NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(ViewController), true);

            ViewController.PushViewController(tabBarController, true);

            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            appDelegate.SetRootViewController(ViewController, true);
        }
    }
}