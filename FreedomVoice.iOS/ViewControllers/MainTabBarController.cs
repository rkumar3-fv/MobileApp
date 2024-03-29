using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.ViewControllers.Texts;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	public partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }
	    public List<ExtensionWithCount> ExtensionsList { private get; set; }

	    public bool IsRootController => NavigationController == null || NavigationController.ViewControllers.Length == 1;

	    public static MainTabBarController SharedInstance;

        public MainTabBarController(IntPtr handle) : base(handle)
        {
            SharedInstance = this;

        }

	    public override void ViewDidLoad()
	    {
            var recentsViewController = AppDelegate.GetViewController<RecentsViewController>();
            var recentsTab = GetTabBarItem(recentsViewController, "Recents");

            var contactsViewController = AppDelegate.GetViewController<ContactsViewController>();
            var contactsTab = GetTabBarItem(contactsViewController, "Contacts");

            var keypadViewController = AppDelegate.GetViewController<KeypadViewController>();
            var keypadTab = GetTabBarItem(keypadViewController, "Keypad");

	        UIViewController messagesTab;
            if (ExtensionsList.Count > 1)
	        {
                var extensionsViewController = AppDelegate.GetViewController<ExtensionsViewController>();
                messagesTab = GetTabBarItem(extensionsViewController, "Voicemail");
            }
            else
            {
                var foldersViewController = AppDelegate.GetViewController<FoldersViewController>();
                foldersViewController.SelectedExtension = ExtensionsList.First();
                foldersViewController.IsSingleExtension = true;
                messagesTab = GetTabBarItem(foldersViewController, "Voicemail");
            }

            var textsControllers = AppDelegate.GetViewController<ListConversationsViewController>();
            var textsTab = GetTabBarItem(textsControllers, "Texts");



            ViewControllers = new[] { recentsTab, contactsTab, keypadTab, messagesTab, textsTab };

            CallerIdEvent.CallerIdChanged += PresentationNumberChanged;

            SelectedIndex = 2;

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationController.NavigationBarHidden = true;

            base.ViewWillAppear(animated);
        }

        public override async void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            (UIApplication.SharedApplication.Delegate as AppDelegate)?.RegisterRemotePushNotifications();
            await FreedomVoice.iOS.Core.Utilities.Helpers.Contacts.GetContactsListAsync();
        }

        public List<PresentationNumber> GetPresentationNumbers()
        {
            return PresentationNumbers;
        }

        public PresentationNumber GetSelectedPresentationNumber()
        {
            if (PresentationNumbers.Count == 0) return null;

            var selectedPresentationNumber = PresentationNumbers.FirstOrDefault(a => a.IsSelected);
            if (selectedPresentationNumber == null)
            {
                selectedPresentationNumber = PresentationNumbers.First();
                selectedPresentationNumber.IsSelected = true;
            }

            return selectedPresentationNumber;
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