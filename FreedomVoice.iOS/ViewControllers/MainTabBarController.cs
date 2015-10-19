using System;
using FreedomVoice.iOS.Entities;
using UIKit;
using FreedomVoice.iOS.Helpers;
using Foundation;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
	    public Account SelectedAccount { get; set; }

        NetworkStatus internetStatus;
        public MainTabBarController(IntPtr handle) : base (handle)
		{

		}

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            NavigationController.NavigationBar.BarTintColor = new UIColor(0.016f, 0.588f, 0.816f, 1);

            UpdateStatus(null, null);
            PhoneCapability.ReachabilityChanged += UpdateStatus;
        }

	    public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);
        }

        void UpdateStatus(object sender, EventArgs e)
        {            
            internetStatus = PhoneCapability.InternetConnectionStatus();            
            NSUserDefaults.StandardUserDefaults.SetInt((int)internetStatus, PhoneCapability.INTERNET_STATUS);            
        }

    }
}