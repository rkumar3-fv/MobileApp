using System;
using Foundation;
using FreedomVoice.iOS.ViewControllers;
using UIKit;

namespace FreedomVoice.iOS
{
    [Register("AppDelegate")]
    public class AppDelegate : UIApplicationDelegate
    {
        public static bool IsAuthenticated
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsAuthenticatedUser"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsAuthenticatedUser");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static bool DisclaimerWasShown
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("DisclaimerWasShown"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "DisclaimerWasShown");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public override UIWindow Window { get; set; }
        public UINavigationController RootNavigationController { get; set; }

        private static UIStoryboard MainStoryboard => UIStoryboard.FromName("MainStoryboard", NSBundle.MainBundle);

        public static T GetViewController<T>(string viewControllerName) where T : class
        {
            return MainStoryboard.InstantiateViewController(viewControllerName) as T;
        }

        public void SetRootViewController(UIViewController rootViewController, bool animate)
        {
            if (animate)
            {
                Window.RootViewController = rootViewController;
                UIView.Transition(Window, 0.5, UIViewAnimationOptions.TransitionFlipFromRight, () => Window.RootViewController = rootViewController, null);
            }
            else
            {
                Window.RootViewController = rootViewController;
            }
        }

        public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
        {
            IsAuthenticated = false;

            if (IsAuthenticated)
                SetAccountsTableViewAsRootView();
            else
                SetLoginViewAsRootView();

            Window.MakeKeyAndVisible();

            return true;
        }

        private void OnLoginSuccess(object sender, EventArgs e)
        {
            IsAuthenticated = true;
            SetAccountsTableViewAsRootView();
        }

        public void GoToLoginScreen()
        {
            IsAuthenticated = false;
            SetLoginViewAsRootView();
        }

        private void SetLoginViewAsRootView()
        {
            RootNavigationController = new UINavigationController();

            var loginViewController = GetViewController<LoginViewController>("LoginViewController");
            loginViewController.OnLoginSuccess += OnLoginSuccess;
            RootNavigationController.PushViewController(loginViewController, false);
            SetRootViewController(RootNavigationController, false);
        }

        private void SetAccountsTableViewAsRootView()
        {
            RootNavigationController = new UINavigationController();

            var accountsTableController = GetViewController<AccountsTableViewController>("AccountsTableViewController");
            RootNavigationController.PushViewController(accountsTableController, false);
            SetRootViewController(RootNavigationController, false);
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

        /// This method is called when the application is about to terminate. Save data, if needed. 
        public override void WillTerminate(UIApplication application) { }
    }
}