using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;
using Foundation;
using System.Collections.Generic;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { private get; set; }

        private bool IsRootController => NavigationController.ViewControllers.Length > 1;

        NetworkStatus _internetStatus;

        UIViewController _recentsTab, _contactsTab, _keypadTab, _messagesTab;

        public List<Recent> Recents { get; set; }

        public MainTabBarController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            Title = SelectedAccount.FormattedPhoneNumber;

            Recents = new List<Recent>() {
                new Recent("Bob Smith", "1234567890", DateTime.Now),
                new Recent(string.Empty, "0987654321", DateTime.Now.AddDays(-1)),
                new Recent(string.Empty, "5556664444", DateTime.Now.AddDays(-2))
            };

            var recentsViewController = AppDelegate.GetViewController<RecentsViewController>();
	        _recentsTab = new UINavigationController(recentsViewController) { TabBarItem = new UITabBarItem(UITabBarSystemItem.Recents, 0) };
            recentsViewController._recents = Recents;

	        var contactsViewController = AppDelegate.GetViewController<ContactsViewController>();
            _contactsTab = contactsViewController;
            _contactsTab.TabBarItem = new UITabBarItem(UITabBarSystemItem.Contacts, 1);

            var keypadViewController = AppDelegate.GetViewController<KeypadViewController>();
	        _keypadTab = new UINavigationController(keypadViewController) { Title = "Keypad" };

	        var messagesViewController = AppDelegate.GetViewController<MessagesViewController>();
	        _messagesTab = new UINavigationController(messagesViewController) { Title = "Messages" };

	        ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            SelectedIndex = 2;

            if (IsRootController)
                NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(NavigationController, "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);

            UpdateStatus(null, EventArgs.Empty);
            PhoneCapability.ReachabilityChanged += UpdateStatus;            
        }

        private void UpdateStatus(object sender, EventArgs e)
        {            
            _internetStatus = PhoneCapability.InternetConnectionStatus();            
            NSUserDefaults.StandardUserDefaults.SetInt((int)_internetStatus, PhoneCapability.INTERNET_STATUS);            
        }
    }
}