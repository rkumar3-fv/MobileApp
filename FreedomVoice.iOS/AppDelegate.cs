using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using FreedomVoice.iOS.Views;
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

        public static int SystemVersion => UIDevice.CurrentDevice.CheckSystemVersion(9, 0) ? 9 : 8;

        public static async Task<bool> ContactHasAccessPermissionsAsync()
        {
            return await new Xamarin.Contacts.AddressBook().RequestPermission();
        }

        public async static Task<List<Contact>> GetContactsListAsync()
        {
            if (await ContactHasAccessPermissionsAsync()) return new Xamarin.Contacts.AddressBook().ToList();

            new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close").Show();
            return null;
        }

        public static AVPlayerView ActivePlayerView;
        public static string ActivePlayerMessageId;

        public static string TempFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "..", "tmp");

        public static T GetViewController<T>() where T : UIViewController
        {
            return (T)MainStoryboard.InstantiateViewController(typeof(T).Name);
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            ServiceContainer.Register(Window);
            ServiceContainer.Register<ISynchronizeInvoke>(() => new SynchronizeInvoke());

            InitializeGoogleAnalytics();
            Theme.Apply();

            _observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);

            if (UserDefault.IsAuthenticated)
                ProceedWithAuthenticatedUser();
            else
                PassToAuthentificationProcess();

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
            UserDefault.LastUsedAccount = string.Empty;

            ActivePlayerView?.StopPlayback();
            ActivePlayerView = null;
            ActivePlayerMessageId = string.Empty;

            RemoveTmpFiles();

            KeyChain.DeletePasswordForUsername(KeyChain.GetUsername());

            PassToAuthentificationProcess();
        }

        private async Task ProceedGetAccountsList()
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Window.RootViewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            var accountsViewModel = new AccountsViewModel(Window.RootViewController);

            await accountsViewModel.GetAccountsListAsync();

            if (accountsViewModel.IsErrorResponseReceived)
                return;

            await PrepareRootView(accountsViewModel.AccountsList);
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
                    Theme.TransitionController(navigationController, false);
                }
            }
            else if (!string.IsNullOrEmpty(UserDefault.LastUsedAccount))
            {
                var accountsController = GetViewController<AccountsViewController>();
                accountsController.AccountsList = accountsList;
                navigationController = new UINavigationController(accountsController);

                var account = GetLastSelectedAccount(accountsList);
                if (account == null)
                    Theme.TransitionController(navigationController);

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController);
                if (mainTabController != null)
                {
                    navigationController.PushViewController(mainTabController, false);
                    Theme.TransitionController(navigationController, false);
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

        private static Account GetLastSelectedAccount(List<Account> accountsList)
        {
            var lastUsedAccount = UserDefault.LastUsedAccount;
            return (from account in accountsList let phoneNumber = account.PhoneNumber where phoneNumber == lastUsedAccount select account).FirstOrDefault();
        }

        private void PassToAuthentificationProcess()
        {
            var loginViewController = GetViewController<LoginViewController>();
            loginViewController.OnLoginSuccess -= OnLoginSuccess;
            loginViewController.OnLoginSuccess += OnLoginSuccess;

            var navigationController = new UINavigationController(loginViewController);
            Theme.TransitionController(navigationController, false);

            Window.MakeKeyAndVisible();
        }

        private async void ProceedWithAuthenticatedUser()
        {
            var splashViewController = GetViewController<SplashViewController>();

            var navigationController = new UINavigationController(splashViewController);
            Theme.TransitionController(navigationController, false);

            Window.MakeKeyAndVisible();

            await ProceedAutoLogin(splashViewController);

            await ProceedGetAccountsList();
        }

        public static async Task ProceedAutoLogin(UIViewController viewController)
        {
            string password = null;

            var userName = KeyChain.GetUsername();
            if (userName != null)
                password = KeyChain.GetPasswordForUsername(userName);

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password)) return;

            var loginViewModel = new LoginViewModel(userName, password, viewController);

            await loginViewModel.AutoLoginAsync();
        }

        private static void RemoveTmpFiles()
        {
            var fileManager = new NSFileManager();
            NSError error;
            var content = fileManager.GetDirectoryContent(TempFolderPath, out error);
            foreach (var file in content)
                fileManager.Remove(Path.Combine(TempFolderPath, file), out error);

            if (error != null)
                Console.Write(error);
        }

        public static async Task<MainTabBarController> GetMainTabBarController(Account selectedAccount, UIViewController viewController)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return null;
            }

            var mainTabBarViewModel = new MainTabBarViewModel(selectedAccount, viewController);
            await mainTabBarViewModel.GetExtensionsListAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            await mainTabBarViewModel.GetPoolingIntervalAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            await mainTabBarViewModel.GetPresentationNumbersAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            var mainTabController = GetViewController<MainTabBarController>();
            mainTabController.SelectedAccount = selectedAccount;
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