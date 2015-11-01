using System;
using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using UIKit;
using System.Linq;

namespace FreedomVoice.iOS.ViewControllers
{
	public partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { private get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }
	    public List<ExtensionWithCount> ExtensionsList { private get; set; }

	    private bool IsRootController => NavigationController.ViewControllers.Length == 1;

        UIViewController _recentsTab, _contactsTab, _keypadTab, _messagesTab;

        public List<Recent> Recents { get; private set; }

	    public static MainTabBarController Instance;

        public MainTabBarController(IntPtr handle) : base(handle)
        {
            Recents = new List<Recent>();
            Instance = this;
        }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            var recentsViewController = AppDelegate.GetViewController<RecentsViewController>();
	        var recentsTabBarImage = UIImage.FromFile("tab_recents.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            var recentsTabBarImageSelected = UIImage.FromFile("tab_recents_active.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
	        _recentsTab = new UINavigationController(recentsViewController);
	        _recentsTab.TabBarItem = new UITabBarItem("Recents", recentsTabBarImage, recentsTabBarImageSelected) { Tag = 0 };

            var contactsViewController = AppDelegate.GetViewController<ContactsViewController>();
            var contactsTabBarImage = UIImage.FromFile("tab_contacts.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            var contactsTabBarImageSelected = UIImage.FromFile("tab_contacts_active.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            _contactsTab = contactsViewController;
            _contactsTab.TabBarItem = new UITabBarItem("Contacts", contactsTabBarImage, contactsTabBarImageSelected) { Tag = 1 };

            var keypadViewController = AppDelegate.GetViewController<KeypadViewController>();
            var keypadTabBarImage = UIImage.FromFile("tab_keypad.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            var keypadTabBarImageSelected = UIImage.FromFile("tab_keypad_active.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
	        _keypadTab = new UINavigationController(keypadViewController);
            _keypadTab.TabBarItem = new UITabBarItem("Keypad", keypadTabBarImage, keypadTabBarImageSelected) { Tag = 2 };

	        var extensionsViewController = AppDelegate.GetViewController<ExtensionsViewController>();
	        extensionsViewController.SelectedAccount = SelectedAccount;
	        extensionsViewController.ExtensionsList = ExtensionsList;
            var messagesTabBarImage = UIImage.FromFile("tab_messages.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            var messagesTabBarImageSelected = UIImage.FromFile("tab_messages_active.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
	        _messagesTab = new UINavigationController(extensionsViewController);
	        _messagesTab.TabBarItem = new UITabBarItem("Messages", messagesTabBarImage, messagesTabBarImageSelected) { Tag = 3 };

            ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            ViewControllerSelected += TabBarOnViewControllerSelected;
            NavigationItem.HidesBackButton = true;
            SelectedIndex = 2;  
            
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            CallerIDEvent.CallerIDChanged += PresentationNumberChanged;
        }

	    private void TabBarOnViewControllerSelected(object sender, EventArgs arg)
	    {
            NavigationItem.SetLeftBarButtonItem(null, false);
	        NavigationItem.SetLeftBarButtonItems(new UIBarButtonItem[] { }, false);

            switch (TabBar.SelectedItem.Tag)
	        {
                case 0:
                    NavigationItem.SetLeftBarButtonItem(Appearance.GetBackButton(NavigationController, "Edit"), true);
                    NavigationItem.SetHidesBackButton(false, false);
                    break;
                case 3:
	                if (IsRootController) NavigationItem.SetHidesBackButton(true, false);
	                else
	                {
                        NavigationItem.SetLeftBarButtonItems(Appearance.GetBackButtonWithArrow(NavigationController, false, "Accounts"), true);
                        NavigationItem.SetHidesBackButton(false, false);
                    }
                    break;
	            default:
                    NavigationItem.SetHidesBackButton(true, false);
                    break;
	        }
	    }

        private void PresentationNumberChanged(object sender, EventArgs args)
        {
            var selectedPresentationNumber = (args as CallerIDEventArgs)?.SelectedPresentationNumber;
            foreach (var item in PresentationNumbers)
            {
                item.IsSelected = item == selectedPresentationNumber;
            }
        }

        public List<PresentationNumber> GetPresentationNumbers()
        {
            return PresentationNumbers;
        }

        public PresentationNumber GetSelectedPresentationNumber()
        {
            return PresentationNumbers.FirstOrDefault(a => a.IsSelected);
        }
    }
}