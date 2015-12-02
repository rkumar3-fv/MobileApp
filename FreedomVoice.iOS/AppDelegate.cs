using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using FreedomVoice.iOS.Views;
using UIKit;
using GoogleAnalytics.iOS;
using Xamarin;
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

        private static bool? HasContactsPermissions { get; set; }
        public static async Task<bool> ContactHasAccessPermissionsAsync()
        {
            return HasContactsPermissions ?? (HasContactsPermissions = await new Xamarin.Contacts.AddressBook().RequestPermission()).Value;
        }

        public async static Task<List<Contact>> GetContactsListAsync()
        {
            if (await ContactHasAccessPermissionsAsync()) return new Xamarin.Contacts.AddressBook().ToList();

            return new List<Contact>();
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

            _observer = NSNotificationCenter.DefaultCenter.AddObserver((NSString)"NSUserDefaultsDidChangeNotification", DefaultsChanged);

            InitializeAnalytics();

            Theme.Apply();

            if (UserDefault.IsAuthenticated)
                ProceedWithAuthenticatedUser();
            else
                PassToAuthentificationProcess();

            return true;
        }

        private async void OnLoginSuccess(object sender, EventArgs e)
        {
            UserDefault.IsAuthenticated = true;

            await ProceedGetAccountsList(true);
        }

        private async void ProceedWithAuthenticatedUser()
        {
            var splashViewController = GetViewController<SplashViewController>();

            var navigationController = new UINavigationController(splashViewController);
            Theme.TransitionController(navigationController, false);

            Window.MakeKeyAndVisible();

            var watcher = Stopwatch.StartNew();

            await ProceedGetAccountsList(false);

            watcher.Stop();
            Log.ReportTime(Log.EventCategory.LongAction, "LoadingScreen", "", watcher.ElapsedMilliseconds);
        }

        public void PassToAuthentificationProcess()
        {
            var loginViewController = GetViewController<LoginViewController>();
            loginViewController.OnLoginSuccess -= OnLoginSuccess;
            loginViewController.OnLoginSuccess += OnLoginSuccess;

            var navigationController = new UINavigationController(loginViewController);
            Theme.TransitionController(navigationController, false);

            Window.MakeKeyAndVisible();
        }

        private async Task ProceedGetAccountsList(bool noCache)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                PassToAuthentificationProcess();
                return;
            }

            var accountsViewModel = new AccountsViewModel { DoNotUseCache = noCache };

            var watcher = Stopwatch.StartNew();
            await accountsViewModel.GetAccountsListAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "GetAccounts", "", watcher.ElapsedMilliseconds);

            if (accountsViewModel.IsErrorResponseReceived)
            {
                PassToAuthentificationProcess();
                return;
            }

            await PrepareRootView(accountsViewModel.AccountsList, noCache);
        }

        private async Task PrepareRootView(List<Account> accountsList, bool noCache)
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

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController, CGPoint.Empty, noCache);
                if (mainTabController == null)
                {
                    PassToAuthentificationProcess();
                    return;
                }

                navigationController = new UINavigationController(mainTabController);
                Theme.TransitionController(navigationController, false);
            }
            else
            if (!string.IsNullOrEmpty(UserDefault.LastUsedAccount))
            {
                var accountsController = GetViewController<AccountsViewController>();
                accountsController.AccountsList = accountsList;
                navigationController = new UINavigationController(accountsController);

                var account = GetLastSelectedAccount(accountsList);
                if (account == null)
                {
                    Theme.TransitionController(navigationController);
                    return;
                }

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController, CGPoint.Empty, noCache);
                if (mainTabController == null)
                {
                    PassToAuthentificationProcess();
                    return;
                }

                navigationController.PushViewController(mainTabController, false);
                Theme.TransitionController(navigationController, false);
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

        public async Task PrepareAuthentificationCookie()
        {
            if (Cookies.HasActiveCookieInContainer())
                return;

            if (Cookies.IsCookieStored)
            {
                Cookies.PutStoredCookieToContainer();
                return;
            }

            string password = null;

            var userName = KeyChain.GetUsername();
            if (!string.IsNullOrEmpty(userName))
                password = KeyChain.GetPasswordForUsername(userName);

            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(password))
            {
                PassToAuthentificationProcess();
                return;
            }

            var loginViewModel = new LoginViewModel(userName, password);

            var watcher = Stopwatch.StartNew();
            await loginViewModel.AutoLoginAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "Login", "", watcher.ElapsedMilliseconds);

            if (loginViewModel.IsErrorResponseReceived)
                PassToAuthentificationProcess();
        }

        public void GoToLoginScreen()
        {
            UserDefault.IsAuthenticated = false;
            UserDefault.RequestCookie = string.Empty;
            UserDefault.LastUsedAccount = string.Empty;
            UserDefault.AccountsCache = string.Empty;
            UserDefault.PresentationPhonesCache = string.Empty;

            ResetAudioPlayer();
            RemoveTmpFiles();

            var username = KeyChain.GetUsername();
            KeyChain.DeletePasswordForUsername(username);

            PassToAuthentificationProcess();
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

        public static void ResetAudioPlayer()
        {
            ActivePlayerView?.StopPlayback();
            ActivePlayerView = null;
            ActivePlayerMessageId = string.Empty;
        }

        public static async Task<MainTabBarController> GetMainTabBarController(Account selectedAccount, UIViewController viewController, CGPoint activityIndicatorCenter, bool noCache)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return null;
            }

            var mainTabBarViewModel = new MainTabBarViewModel(selectedAccount, viewController) { DoNotUseCache = noCache, ActivityIndicatorCenter = activityIndicatorCenter };

            var watcher = Stopwatch.StartNew();
            await mainTabBarViewModel.GetPresentationNumbersAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "GetPresentationNumbers", "", watcher.ElapsedMilliseconds);
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            watcher = Stopwatch.StartNew();
            await mainTabBarViewModel.GetPoolingIntervalAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "GetPoolingInterval", "", watcher.ElapsedMilliseconds);
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            var extensionsViewModel = new ExtensionsViewModel(selectedAccount) { ActivityIndicatorCenter = activityIndicatorCenter };

            watcher = Stopwatch.StartNew();
            await extensionsViewModel.GetExtensionsListAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "GetExtensions", "", watcher.ElapsedMilliseconds);
            if (extensionsViewModel.IsErrorResponseReceived) return null;

            var mainTabController = GetViewController<MainTabBarController>();
            mainTabController.SelectedAccount = selectedAccount;
            mainTabController.PresentationNumbers = mainTabBarViewModel.PresentationNumbers;
            mainTabController.ExtensionsList = extensionsViewModel.ExtensionsList;

            return mainTabController;
        }

        private static void DefaultsChanged(NSNotification obj)
        {
            UserDefault.UpdateFromPreferences();
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
            if (_observer == null) return;

            NSNotificationCenter.DefaultCenter.RemoveObserver(_observer);
            _observer = null;
        }

        private static void InitializeAnalytics()
        {
            InitializeGoogleAnalytics();
            InitializeXamarinInsights();
        }

        private const string GoogleAnalyticsTrackingId = "UA-587407-96";
        private static void InitializeGoogleAnalytics()
        {
            GAI.SharedInstance.DispatchInterval = 20;
            GAI.SharedInstance.TrackUncaughtExceptions = true;
            GAI.SharedInstance.GetTracker(GoogleAnalyticsTrackingId);
        }

        private const string InsightsApiKey = "d3d8eb1b7ea6654b812ec9a8dea5fb8224e3a2b5";
        private static void InitializeXamarinInsights()
        {
            Insights.Initialize(InsightsApiKey);
        }
    }
}