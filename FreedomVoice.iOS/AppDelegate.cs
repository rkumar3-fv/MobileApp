﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using FreedomVoice.iOS.Views;
using GoogleAnalytics.iOS;
using UIKit;
using Xamarin;

namespace FreedomVoice.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        public static int SystemVersion => UIDevice.CurrentDevice.CheckSystemVersion(9, 0) ? 9 : 8;

        public static int RecentsCount => RecentsList.Count;
        public static List<Recent> RecentsList { get; set; }

        public static AVPlayerView ActivePlayerView;
        public static UIButton ActiveSpeakerButton;
        public static string ActivePlayerMessageId;

        public static CancellationTokenSource ActiveDownloadCancelationToken;

        public static ActivityIndicator ActivityIndicator { get; private set; }

        public static string TempFolderPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments, Environment.SpecialFolderOption.DoNotVerify), "..", "tmp");

        private static UIStoryboard MainStoryboard => UIStoryboard.FromName("MainStoryboard", NSBundle.MainBundle);

        public static T GetViewController<T>() where T : UIViewController
        {
            return (T)MainStoryboard.InstantiateViewController(typeof(T).Name);
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            Window = new UIWindow(UIScreen.MainScreen.Bounds);

            ActivityIndicator = new ActivityIndicator(Theme.ScreenBounds);

            Recents.RestoreRecentsFromCache();

            ServiceContainer.Register(Window);
            ServiceContainer.Register<ISynchronizeInvoke>(() => new SynchronizeInvoke());

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

            var viewModel = new PoolingIntervalViewModel();
            await viewModel.GetPoolingIntervalAsync();

            await ProceedGetAccountsList(true);
        }

        private async void ProceedWithAuthenticatedUser()
        {
            var splashViewController = GetViewController<SplashViewController>();
            Theme.TransitionController(splashViewController, false);

            Window.MakeKeyAndVisible();

            var watcher = Stopwatch.StartNew();

            await PrepareAuthentificationCookie();
            await ProceedGetAccountsList(false);

            watcher.Stop();

            Log.ReportTime(Log.EventCategory.LongAction, "LoadingScreen", "", watcher.ElapsedMilliseconds);
        }

        public async void PassToAuthentificationProcess()
        {
            UserDefault.IsAuthenticated = false;
            UserDefault.LastUsedAccount = string.Empty;

            Recents.ClearRecents();

            var loginViewModel = new LoginViewModel();
            await loginViewModel.LogoutAsync();

            NSUserDefaults.StandardUserDefaults.Synchronize();

            ResetAudioPlayer();
            RemoveTmpFiles();

            var storedUsername = KeyChain.GetUsername();
            if (storedUsername != null)
                KeyChain.DeletePasswordForUsername(storedUsername);

            var loginViewController = GetViewController<LoginViewController>();
            loginViewController.OnLoginSuccess -= OnLoginSuccess;
            loginViewController.OnLoginSuccess += OnLoginSuccess;

            Theme.TransitionController(loginViewController, false);

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
            await accountsViewModel.GetAccountsListAsync();
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

                    navigationController = new UINavigationController(phoneNumberController);
                    Theme.TransitionController(navigationController);
                    return;
                }

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController, noCache);
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

                var mainTabController = await GetMainTabBarController(account, Window.RootViewController, noCache);
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

        private static Account GetLastSelectedAccount(IEnumerable<Account> accountsList)
        {
            var lastUsedAccount = UserDefault.LastUsedAccount;
            return (from account in accountsList let phoneNumber = account.PhoneNumber where phoneNumber == lastUsedAccount select account).FirstOrDefault();
        }

        private async Task PrepareAuthentificationCookie()
        {
            if (Cookies.HasActiveCookie())
                return;

            await LoginWithStoredCredentials();
        }

        public async Task LoginWithStoredCredentials()
        {
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
            await loginViewModel.AutoLoginAsync();
            if (loginViewModel.IsErrorResponseReceived)
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
            ActiveSpeakerButton = null;
            ActivePlayerMessageId = string.Empty;
        }

        public static void CancelActiveDownload()
        {
            ActiveDownloadCancelationToken?.Cancel();
            ActiveDownloadCancelationToken = null;
        }

        public static async Task<MainTabBarController> GetMainTabBarController(Account selectedAccount, UIViewController viewController, bool noCache)
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return null;
            }

            var mainTabBarViewModel = new PresentationPhonesViewModel(selectedAccount, viewController) { DoNotUseCache = noCache };
            await mainTabBarViewModel.GetPresentationNumbersAsync();
            if (mainTabBarViewModel.IsErrorResponseReceived) return null;

            var extensionsViewModel = new ExtensionsViewModel(selectedAccount);
            await extensionsViewModel.GetExtensionsListAsync();
            if (extensionsViewModel.IsErrorResponseReceived) return null;

            var mainTabController = GetViewController<MainTabBarController>();
            mainTabController.SelectedAccount = selectedAccount;
            mainTabController.PresentationNumbers = mainTabBarViewModel.PresentationNumbers;
            mainTabController.ExtensionsList = extensionsViewModel.ExtensionsList;

            return mainTabController;
        }

        private static UIViewController GetVisibleViewController(UIViewController rootViewController)
        {
            if (rootViewController == null)
                return null;

            var rootNavigationController = rootViewController as UINavigationController;
            if (rootNavigationController == null) return rootViewController;

            var tabBarController = rootNavigationController.TopViewController as UITabBarController;
            var navigationController = tabBarController?.SelectedViewController as UINavigationController;

            return navigationController != null ? navigationController.TopViewController : rootNavigationController.TopViewController;
        }

        public override void OnActivated(UIApplication application)
        {
            EnableUserInteraction(application);
        }

        public static void EnableUserInteraction(UIApplication application)
        {
            ChangeUserInteractionState(application, true);
        }

        public static void DisableUserInteraction(UIApplication application)
        {
            ChangeUserInteractionState(application, false);
        }

        private static void ChangeUserInteractionState(UIApplication application, bool enabled)
        {
            if (application.KeyWindow == null)
                return;

            var visibleViewController = GetVisibleViewController(application.KeyWindow.RootViewController);
            if (visibleViewController != null)
                visibleViewController.View.UserInteractionEnabled = enabled;
        }

        // This method is invoked when the application is about to move from active to inactive state.
        public override void OnResignActivation(UIApplication application) { }

        // This method should be used to release shared resources and it should store the application state.
        // If your application supports background exection this method is called instead of WillTerminate when the user quits.
        public override void DidEnterBackground(UIApplication application)
        {
            Recents.StoreRecentsToCache();

            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        /// This method is called as part of the transiton from background to active state.
        public override async void WillEnterForeground(UIApplication application)
        {
            await PrepareAuthentificationCookie();

            Recents.RestoreRecentsFromCache();
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