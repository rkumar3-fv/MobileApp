using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using FreedomVoice.Core;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.Core.Entities.Enums;
using UIKit;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class LoginViewController : UIViewController
	{
        public LoginViewController (IntPtr handle) : base (handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

            UsernameTextField.ShouldReturn += (field) => { field.ResignFirstResponder(); return true; };
            PasswordTextField.ShouldReturn += (field) => { field.ResignFirstResponder(); return true; };

            View.AddLinearGradientToView(new UIColor(0, 0.231f, 0.424f, 1), new UIColor(0.855f, 0.949f, 0.965f, 1));

            UsernameTextField.BorderStyle = UITextBorderStyle.RoundedRect;
            PasswordTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            LoginButton.Layer.CornerRadius = 5;
            LoginButton.ClipsToBounds = true;
        }

	    async partial void LoginButton_TouchUpInside(UIButton sender)
	    {
            var result = await ApiHelper.Login("freedomvoice.user1.267055@gmail.com","user1654654");
            //var result = await ApiHelper.Login(UsernameTextField.Text.Trim(), PasswordTextField.Text.Trim());

            switch (result.Code)
            {
                case ErrorCodes.Ok:
                    OnLoginSuccess();
                    break;
                case ErrorCodes.ConnectionLost:
                    new UIAlertView("Login Error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
                    break;
                case ErrorCodes.Unauthorized:
                    InvokeOnMainThread(() => {
                        PasswordValidationLabel.Hidden = false;
                        PasswordTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
                    });
                    PasswordTextField.BecomeFirstResponder();
                    break;
                case ErrorCodes.BadRequest:
                    InvokeOnMainThread(() => {
                        UsernameValidationLabel.Hidden = false;
                        UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
                    });
                    UsernameTextField.BecomeFirstResponder();
                    break;
            }
        }

        async void OnLoginSuccess()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            var mainStoryboard = appDelegate.MainStoryboard;

            //AppDelegate.IsAuthenticated = true;

            var rootNavigationController = new UINavigationController();

            var systems = await ApiHelper.GetSystems();
            List<Account> accounts = systems.Result.PhoneNumbers.Select(phoneNumber => new Account { PhoneNumber = phoneNumber }).ToList();

            if (accounts.Count > 1)
            {
                var accountsTableController = appDelegate.GetViewController(mainStoryboard, "AccountsTableViewController") as AccountsTableViewController;
                if (accountsTableController != null)
                    accountsTableController.Accounts = accounts;

                rootNavigationController.PushViewController(accountsTableController, false);
                appDelegate.SetRootViewController(rootNavigationController, true);
            }
            else
            {
                var tabBarController = appDelegate.GetViewController(mainStoryboard, "MainTabBarController") as MainTabBarController;
                tabBarController.SelectedAccount = accounts.First();

                rootNavigationController.PushViewController(tabBarController, false);
                appDelegate.SetRootViewController(rootNavigationController, true);
            }
        }

        public override void ViewWillAppear(bool animated)
	    {
            NavigationController.NavigationBar.Hidden = true;
            UsernameValidationLabel.Hidden = true;
            PasswordValidationLabel.Hidden = true;

            base.ViewWillAppear(animated);
	    }

        public override void ViewWillDisappear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = false;

            base.ViewWillAppear(animated);
        }        
    }
}