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

        UIViewController _recentsTab, _contactsTab, _keypadTab, _messagesTab;

        public List<Recent> Recents { get; set; }

        public MainTabBarController(IntPtr handle) : base(handle) {
            Recents = new List<Recent>();
        }

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
	        _keypadTab = new UINavigationController(keypadViewController) { Title = "Keypad" };

	        var messagesViewController = AppDelegate.GetViewController<MessagesViewController>();
	        _messagesTab = new UINavigationController(messagesViewController) { Title = "Messages" };

	        ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            SelectedIndex = 2;

            if (IsRootController)
                NavigationItem.SetLeftBarButtonItem(Appearance.GetBackBarButton(NavigationController, "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);
        }
    }
}