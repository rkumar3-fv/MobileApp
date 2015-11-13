using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using GoogleAnalytics.iOS;
using Xamarin.Contacts;

namespace FreedomVoice.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        private static UIStoryboard MainStoryboard => UIStoryboard.FromName("MainStoryboard", NSBundle.MainBundle);
        private NSObject _observer;

        private static readonly Xamarin.Contacts.AddressBook AddressBook = new Xamarin.Contacts.AddressBook();

        public static async Task<bool> ContactHasAccessPermissionsAsync()
        {
            return await AddressBook.RequestPermission();
        }

        public async static Task<List<Contact>> GetContactsListAsync()
        {
            if (await ContactHasAccessPermissionsAsync()) return AddressBook.ToList();

            new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close").Show();
            return null;
        }

        public static T GetViewController<T>() where T : UIViewController
        {
            return (T)MainStoryboard.InstantiateViewController(typeof(T).Name);
        }

        private void SetRootViewController(UIViewController rootViewController, bool animate)
        {
            Window.RootViewController = rootViewController;

            if (animate)
                UIView.Transition(Window, .3, UIViewAnimationOptions.TransitionFlipFromRight, () => Window.RootViewController = rootViewController, delegate { });
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            UserDefault.IsAuthenticated = false;

            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            ServiceContainer.Register(Window);
            ServiceContainer.Register<ISynchronizeInvoke>(() => new SynchronizeInvoke());

            InitializeGoogleAnalytics();
            Theme.Apply();

            if (UserDefault.IsAuthenticated)
                ProceedWithAuthenticatedUser();
            else
                SetLoginViewAsRootView();

            _observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);

            Window.MakeKeyAndVisible();

            return true;
        }

        private async void OnLoginSuccess(object sender, EventArgs e)
        {
            UserDefault.IsAuthenticated = true;

            await ProceedGetAccountsList();
        }

        public void GoToLoginScreen()
        {
            UserDefault.IsAuthenticated = false;
            KeyChain.DeletePasswordForUsername(KeyChain.GetUsername());

            SetLoginViewAsRootView();
        }

        private AccountsViewModel _accountsViewModel;

        private async Task ProceedGetAccountsList()
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowNetworkUnreachableAlert(Window.RootViewController);
                return;
            }

            _accountsViewModel = new AccountsViewModel(Window.RootViewController);

            await _accountsViewModel.GetAccountsListAsync();

            if (_accountsViewModel.IsErrorResponseReceived)
                return;

            await PrepareRootView(_accountsViewModel.AccountsList);
        }

        private async Task PrepareRootView(List<Account> accountsList)
        {
            UINavigationController navigationController;

            if (accountsList.Count == 1)
            {
                var account = accountsList.First();

                if (!UserDefault.IsLaunchedBefore)
                {
                    var phoneNumberController = GetViewController<PhoneNumberViewController>();
                    phoneNumberController.SelectedAccount = account;
                    phoneNumberController.ParentController = Window.RootViewController;

                    navigationController = new UINavigationController(phoneNumberController);
                    Theme.TransitionController(navigationController);

                    return;
                }

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController);
                if (mainTabController != null)
                {
                    navigationController = new UINavigationController(mainTabController);
                    Theme.TransitionController(navigationController);
                }
            }
            else
            {
                var accountsController = GetViewController<AccountsViewController>();
                accountsController.AccountsList = accountsList;
                navigationController = new UINavigationController(accountsController);
                Theme.TransitionController(navigationController);
            }
        }

        private void SetLoginViewAsRootView()
        {
            var loginViewController = GetViewController<LoginViewController>();
            loginViewController.OnLoginSuccess -= OnLoginSuccess;
            loginViewController.OnLoginSuccess += OnLoginSuccess;

            var navigationController = new UINavigationController(loginViewController);
            SetRootViewController(navigationController, false);
        }

        private async void ProceedWithAuthenticatedUser()
        {
            //TODO: Create splash screen navigation controller and make it root controller

            await ProceedGetAccountsList();

        }

        public static async Task<MainTabBarController> GetMainTabBarController(Account selectetdAccount, UIViewController viewController)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowNetworkUnreachableAlert(viewController);
                return null;
            }

            var mainTabBarViewModel = new MainTabBarViewModel(selectetdAccount, viewController);
            await mainTabBarViewModel.GetExtensionsListAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            await mainTabBarViewModel.GetPresentationNumbersAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            var mainTabController = GetViewController<MainTabBarController>();
            mainTabController.SelectedAccount = selectetdAccount;
            mainTabController.PresentationNumbers = mainTabBarViewModel.PresentationNumbers;
            mainTabController.ExtensionsList = mainTabBarViewModel.ExtensionsList;

            return mainTabController;
        }

        // This method is invoked when the application is about to move from active to inactive state.
        // OpenGL applications should use this method to pause.
        public override void OnResignActivation(UIApplication application) { }

        // This method should be used to release shared resources and it should store the application state.
        // If your application supports background exection this method is called instead of WillTerminate
        // when the user quits.
        public override void DidEnterBackground(UIApplication application) { }

        /// This method is called as part of the transiton from background to active state.
        public override void WillEnterForeground(UIApplication application) { }

        public override void WillTerminate(UIApplication application)
        {
            base.WillTerminate(application);
            if (_observer == null) return;

            NSNotificationCenter.DefaultCenter.RemoveObserver(_observer);
            _observer = null;
        }

        public IGAITracker Tracker;
        private const string TrackingId = "UA-587407-96";

        private void InitializeGoogleAnalytics()
        {
            GAI.SharedInstance.DispatchInterval = 20;                        
            GAI.SharedInstance.TrackUncaughtExceptions = true;                        
            Tracker = GAI.SharedInstance.GetTracker(TrackingId);            
        }

        private static void DefaultsChanged(NSNotification obj)
        {
            UserDefault.UpdateFromPreferences();
        }
    }
}