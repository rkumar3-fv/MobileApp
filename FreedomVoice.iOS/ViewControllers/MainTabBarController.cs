using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;
using Foundation;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { private get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }

        private bool IsRootController => NavigationController.ViewControllers.Length > 1;

        NetworkStatus _internetStatus;

        UIViewController _recentsTab, _contactsTab, _keypadTab, _messagesTab;

        public MainTabBarController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            Title = SelectedAccount.FormattedPhoneNumber;

            var recentsViewController = AppDelegate.GetViewController<RecentsViewController>();
	        _recentsTab = new UINavigationController(recentsViewController) { TabBarItem = new UITabBarItem(UITabBarSystemItem.Recents, 0) };

            var contactsViewController = AppDelegate.GetViewController<ContactsViewController>();
            _contactsTab = contactsViewController;
            _contactsTab.TabBarItem = new UITabBarItem(UITabBarSystemItem.Contacts, 1);

            var keypadViewController = AppDelegate.GetViewController<KeypadViewController>();
	        _keypadTab = new UINavigationController(keypadViewController) { TabBarItem = new UITabBarItem("Keypad", null, 2) };

	        var messagesViewController = AppDelegate.GetViewController<MessagesViewController>();
	        _messagesTab = new UINavigationController(messagesViewController) { TabBarItem = new UITabBarItem("Messages", null, 3) };

            ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            ViewControllerSelected += TabBarOnViewControllerSelected;
            NavigationItem.HidesBackButton = true;
            SelectedIndex = 2;
            
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);

            UpdateStatus(null, EventArgs.Empty);
            PhoneCapability.ReachabilityChanged += UpdateStatus;
        }

	    private void TabBarOnViewControllerSelected(object sender, EventArgs arg)
	    {
            NavigationItem.LeftBarButtonItem = null;

            switch (TabBar.SelectedItem.Tag)
	        {
                case 0:
                    NavigationItem.SetLeftBarButtonItems(new[] { Appearance.GetBackBarButton(NavigationController, "Edit") }, true);
                    NavigationItem.SetHidesBackButton(false, true);
                    break;
                case 3:
                    if (IsRootController) NavigationItem.SetLeftBarButtonItems(new[] { Appearance.GetBackBarButton(NavigationController, "Accounts") }, true);
                    NavigationItem.SetHidesBackButton(false, true);
                    break;
                default:
                    NavigationItem.SetHidesBackButton(true, true);
                    break;
	        }
	    }

        private void UpdateStatus(object sender, EventArgs e)
        {            
            _internetStatus = PhoneCapability.InternetConnectionStatus();            
            NSUserDefaults.StandardUserDefaults.SetInt((int)_internetStatus, PhoneCapability.INTERNET_STATUS);            
        }
    }
}