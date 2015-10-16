using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Foundation;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewControllers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public override UIWindow Window { get; set; }

        private static UIStoryboard MainStoryboard => UIStoryboard.FromName("MainStoryboard", NSBundle.MainBundle);

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

            return true;
        }

        private void OnLoginSuccess(object sender, EventArgs e)
        {
            UserDefault.IsAuthenticated = true;

            ProceedGetAccountsList();
        }

        public void GoToLoginScreen()
        {
            UserDefault.IsAuthenticated = false;
            SetLoginViewAsRootView();
        }

        private AccountsViewModel _accountsViewModel;

        private async void ProceedGetAccountsList()
        {
            _accountsViewModel = ServiceContainer.Resolve<AccountsViewModel>();
            await _accountsViewModel.GetAccountsListAsync().ContinueWith(t => BeginInvokeOnMainThread(() => { OnProceedGetAccountsListResponse(t); }));
        }

        private static void OnProceedGetAccountsListResponse(Task<BaseResult<DefaultPhoneNumbers>> task)
        {
            var baseResult = task.Result;
            switch (baseResult.Code)
            {
                case ErrorCodes.Ok:
                    PrepareRootView(baseResult.Result);
                    break;
                case ErrorCodes.ConnectionLost:
                    new UIAlertView("Accounts Retrieve Error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
                    break;
                case ErrorCodes.Unauthorized:
                case ErrorCodes.BadRequest:
                    //TODO: Relogin user
                    break;
            }
        }

        private static void PrepareRootView(DefaultPhoneNumbers phoneNumbers)
        {
            var accountsList = phoneNumbers.PhoneNumbers.Select(phoneNumber => new Account { PhoneNumber = phoneNumber }).ToList();

            if (accountsList.Count == 1)
            {
                var mainTabController = GetViewController<MainTabBarController>();
                mainTabController.SelectedAccount = accountsList.First();
                var navigationController = new UINavigationController(mainTabController);
                Theme.TransitionController(navigationController);
            }
            else
            {
                var accountsController = GetViewController<AccountsViewController>();
                accountsController.AccountsList = accountsList;
                var navigationController = new UINavigationController(accountsController);
                Theme.TransitionController(navigationController);
            }
        }

        private void SetLoginViewAsRootView()
        {
            var loginViewController = GetViewController<LoginViewController>();
            loginViewController.OnLoginSuccess += OnLoginSuccess;
            var navigationController = new UINavigationController(loginViewController);
            SetRootViewController(navigationController, false);
        }

        private void SetAccountsViewAsRootView()
        {
            var accountsController = GetViewController<AccountsViewController>();
            SetRootViewController(accountsController, false);
        }

        // This method is invoked when the application is about to move from active to inactive state.
        // OpenGL applications should use this method to pause.
        public override void OnResignActivation(UIApplication application) { }

        // This method should be used to release shared resources and it should store the application state.
        // If your application supports background exection this method is called instead of WillTerminate
        // when the user quits.
        public override void DidEnterBackground(UIApplication application)
        {
            
        }

        /// This method is called as part of the transiton from background to active state.
        public override void WillEnterForeground(UIApplication application)
        {
            
        }
    }
}