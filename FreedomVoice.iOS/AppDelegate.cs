using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using GoogleAnalytics.iOS;

namespace FreedomVoice.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        private static UIStoryboard MainStoryboard => UIStoryboard.FromName("MainStoryboard", NSBundle.MainBundle);
        NSObject observer;

        public static T GetViewController<T>() where T : UIViewController
        {
            return (T)MainStoryboard.InstantiateViewController(typeof(T).Name);
        }

        public void SetRootViewController(UIViewController rootViewController, bool animate)
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

            //Apply our UI theme
            Theme.Apply();

            if (UserDefault.IsAuthenticated)
                SetAccountsViewAsRootView();
            else
                SetLoginViewAsRootView();

            Window.MakeKeyAndVisible();

            InitGA();

            observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);

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
            SetLoginViewAsRootView();
        }

        private AccountsViewModel _accountsViewModel;

        private async Task ProceedGetAccountsList()
        {
            _accountsViewModel = ServiceContainer.Resolve<AccountsViewModel>();
            await _accountsViewModel.GetAccountsListAsync();
            if (_accountsViewModel.IsErrorResponseReceived) return;

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

        private void SetAccountsViewAsRootView()
        {
            var accountsController = GetViewController<AccountsViewController>();

            var navigationController = new UINavigationController(accountsController);
            SetRootViewController(navigationController, false);
        }

        public static async Task<MainTabBarController> GetMainTabBarController(Account selectetdAccount, UIViewController viewController)
        {
            var extensionsViewModel = new ExtensionsViewModel(selectetdAccount, viewController as UINavigationController);
            await extensionsViewModel.GetExtensionsListAsync();
            if (extensionsViewModel.IsErrorResponseReceived) return null;

            var presentationNumbersViewModel = new PresentationNumbersViewModel(selectetdAccount.PhoneNumber);
            await presentationNumbersViewModel.GetPresentationNumbersAsync();
            if (presentationNumbersViewModel.IsErrorResponseReceived) return null;

            var mainTabController = GetViewController<MainTabBarController>();
            mainTabController.SelectedAccount = selectetdAccount;
            mainTabController.PresentationNumbers = presentationNumbersViewModel.PresentationNumbers;
            mainTabController.ExtensionsList = extensionsViewModel.ExtensionsList;

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
            if (observer != null)
            {
                NSNotificationCenter.DefaultCenter.RemoveObserver(observer);
                observer = null;
            }
        }

        public IGAITracker Tracker;
        public static readonly string TrackingId = "UA-69040520-2";        

        private void InitGA()
        {
            GAI.SharedInstance.DispatchInterval = 20;                        
            GAI.SharedInstance.TrackUncaughtExceptions = true;                        
            Tracker = GAI.SharedInstance.GetTracker(TrackingId);            
        }

        void DefaultsChanged(NSNotification obj)
        {
            Settings.UpdateFromPreferences();
        }
    }
}