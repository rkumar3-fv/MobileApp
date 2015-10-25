using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { private get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }

        private bool IsRootController => NavigationController.ViewControllers.Length > 1;

        UIViewController _recentsTab, _contactsTab, _keypadTab, _messagesTab;

        public List<Recent> Recents { get; set; }

        public MainTabBarController(IntPtr handle) : base(handle)
        {
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
	        _keypadTab = new UINavigationController(keypadViewController) { TabBarItem = new UITabBarItem("Keypad", null, 2) };

	        var extensionsViewController = AppDelegate.GetViewController<ExtensionsViewController>();
	        extensionsViewController.SelectedAccount = SelectedAccount;
            _messagesTab = new UINavigationController(extensionsViewController) { TabBarItem = new UITabBarItem("Messages", null, 3) };

            ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            ViewControllerSelected += TabBarOnViewControllerSelected;
            NavigationItem.HidesBackButton = true;
            SelectedIndex = 2;
            
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(), true);
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
    }
}