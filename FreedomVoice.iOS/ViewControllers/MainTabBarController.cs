using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities.Events;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	public partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }
	    public List<ExtensionWithCount> ExtensionsList { private get; set; }

	    public bool IsRootController => NavigationController.ViewControllers.Length == 1;

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
            var recentsViewController = AppDelegate.GetViewController<RecentsViewController>();
            _recentsTab = GetTabBarItem(recentsViewController, "Recents");

            var contactsViewController = AppDelegate.GetViewController<ContactsViewController>();
            _contactsTab = GetTabBarItem(contactsViewController, "Contacts");

            var keypadViewController = AppDelegate.GetViewController<KeypadViewController>();
            _keypadTab = GetTabBarItem(keypadViewController, "Keypad");

	        var extensionsViewController = AppDelegate.GetViewController<ExtensionsViewController>();
	        extensionsViewController.SelectedAccount = SelectedAccount;
	        extensionsViewController.ExtensionsList = ExtensionsList;
            _messagesTab = GetTabBarItem(extensionsViewController, "Messages");

	        ViewControllers = new[] { _recentsTab, _contactsTab, _keypadTab, _messagesTab };

            SelectedIndex = 2;  

            CallerIdEvent.CallerIdChanged += PresentationNumberChanged;

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationController.NavigationBarHidden = true;
            base.ViewWillAppear(animated);
        }

        public List<PresentationNumber> GetPresentationNumbers()
        {
            return PresentationNumbers;
        }

        public PresentationNumber GetSelectedPresentationNumber()
        {
            if (PresentationNumbers.FirstOrDefault(a => a.IsSelected) == null && PresentationNumbers.Count > 0)
                PresentationNumbers[0].IsSelected = true;

            return PresentationNumbers.FirstOrDefault(a => a.IsSelected);
        }

        private static UIViewController GetTabBarItem(UIViewController viewController, string tabTitle)
        {
            var tabImage = UIImage.FromFile($"tab_{tabTitle.ToLower()}.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            var tabImageSelected = UIImage.FromFile($"tab_{tabTitle.ToLower()}_active.png")?.ImageWithRenderingMode(UIImageRenderingMode.AlwaysOriginal);
            return new UINavigationController(viewController) { TabBarItem = new UITabBarItem(tabTitle, tabImage, tabImageSelected) };
        }

        private void PresentationNumberChanged(object sender, EventArgs args)
        {
            var selectedPresentationNumber = (args as CallerIdEventArgs)?.SelectedPresentationNumber;
            foreach (var item in PresentationNumbers)
                item.IsSelected = item == selectedPresentationNumber;
        }
    }
}