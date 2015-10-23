using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class EmergencyDisclaimerViewController : UIViewController
	{
	    public Account SelectedAccount { private get; set; }
        public UINavigationController ParentController { private get; set; }

        public EmergencyDisclaimerViewController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            UnderstandButton.Layer.CornerRadius = 5;
            UnderstandButton.ClipsToBounds = true;
        }

	    partial void UnderstandButton_TouchUpInside(UIButton sender)
	    {
	        UserDefault.DisclaimerWasShown = true;

            var tabBarController = AppDelegate.GetViewController<MainTabBarController>();
            tabBarController.SelectedAccount = SelectedAccount;
            ParentController.PushViewController(tabBarController, true);
            Theme.TransitionController(ParentController);
        }
    }
}