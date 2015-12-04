using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using GoogleAnalytics.iOS;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
	public partial class MainTabBarController : UITabBarController
	{
        public Account SelectedAccount { get; set; }
        public List<PresentationNumber> PresentationNumbers { private get; set; }
	    public List<ExtensionWithCount> ExtensionsList { private get; set; }

	    public bool IsRootController => NavigationController.ViewControllers.Length == 1;

	    private List<Recent> Recents { get; }
	    public List<Contact> Contacts { get; set; }

	    public int RecentsCount => Recents.Count;
        public int ContactsCount => Contacts.Count;

	    public static MainTabBarController SharedInstance;

        public MainTabBarController(IntPtr handle) : base(handle)
        {
            SharedInstance = this;

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Main Tab Bar Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());

            Recents = new List<Recent>();
            Contacts = new List<Contact>();
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
                messagesTab = GetTabBarItem(extensionsViewController, "Messages");
            }
            else
            {
                var foldersViewController = AppDelegate.GetViewController<FoldersViewController>();
                foldersViewController.SelectedExtension = ExtensionsList.First();
                foldersViewController.IsSingleExtension = true;
                messagesTab = GetTabBarItem(foldersViewController, "Messages");
            }
            
            ViewControllers = new[] { recentsTab, contactsTab, keypadTab, messagesTab };

            AppDelegate.ActivityIndicator.SetActivityIndicatorCenter(Theme.ScreenCenter);

            CallerIdEvent.CallerIdChanged += PresentationNumberChanged;

            SelectedIndex = 2;

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
            if (PresentationNumbers.Count == 0) return null;

            var selectedPresentationNumber = PresentationNumbers.FirstOrDefault(a => a.IsSelected);
            if (selectedPresentationNumber == null)
            {
                selectedPresentationNumber = PresentationNumbers.First();
                selectedPresentationNumber.IsSelected = true;
            }

            return selectedPresentationNumber;
        }

	    public void AddRecent(Recent recent)
	    {
            var existingRecent = Recents.FirstOrDefault(r => DataFormatUtils.NormalizePhone(r.PhoneNumber) == DataFormatUtils.NormalizePhone(recent.PhoneNumber));
            if (existingRecent == null)
                Recents.Add(recent);
            else
            {
                existingRecent.DialDate = DateTime.Now;
                existingRecent.CallsQuantity++;
            }
        }

	    public void ClearRecents()
	    {
	        Recents.Clear();
	    }

        public void RemoveRecents(Recent recent)
        {
            Recents.Remove(recent);
        }

        public List<Recent> GetRecentsOrdered()
        {
            return Recents.OrderByDescending(r => r.DialDate).ToList();
        }

        public Recent GetLastRecent()
        {
            return GetRecentsOrdered().First();
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