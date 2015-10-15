using System;
using CoreGraphics;
using FreedomVoice.Core;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.Core.Entities.Enums;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class LoginViewController : UIViewController
	{
        public event EventHandler OnLoginSuccess;

        private LoadingOverlay _loadingOverlay;

        public LoginViewController (IntPtr handle) : base (handle) { }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

	        Title = "Login";

            UsernameTextField.ShouldReturn += (field) => { field.ResignFirstResponder(); return true; };
            PasswordTextField.ShouldReturn += (field) => { field.ResignFirstResponder(); return true; };

            UsernameTextField.Text = "freedomvoice.user1.267055@gmail.com";
            PasswordTextField.Text = "user1654654";

            View.AddLinearGradientToView(new UIColor(0, 0.231f, 0.424f, 1), new UIColor(0.855f, 0.949f, 0.965f, 1));

            UsernameTextField.BorderStyle = PasswordTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            LoginButton.Layer.CornerRadius = 5;
            LoginButton.ClipsToBounds = true;
        }

	    async partial void LoginButton_TouchUpInside(UIButton sender)
	    {
	        var username = UsernameTextField.Text.Trim();
	        var password = PasswordTextField.Text.Trim();

            if (!Validation.IsValidEmail(username))
	        {
                InvokeOnMainThread(() => { UsernameValidationLabel.Hidden = false; UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1); });
                UsernameTextField.BecomeFirstResponder();
            }

            if (password == string.Empty)
            {
                InvokeOnMainThread(() => { PasswordValidationLabel.Hidden = false; PasswordTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1); });
                PasswordTextField.BecomeFirstResponder();
            }

            ShowOverlay();
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            var result = await ApiHelper.Login(username, password);
            switch (result.Code)
            {
                case ErrorCodes.Ok:
                    OnLoginSuccess?.Invoke(sender, new EventArgs());
                    break;
                case ErrorCodes.ConnectionLost:
                    new UIAlertView("Login Error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
                    break;
                case ErrorCodes.Unauthorized:
                case ErrorCodes.BadRequest:
                    InvokeOnMainThread(() => { UsernameValidationLabel.Hidden = false; UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1); });
                    UsernameTextField.BecomeFirstResponder();
                    break;
            }

            HideOverlay();
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        private void ShowOverlay()
        {
            _loadingOverlay = new LoadingOverlay(UIScreen.MainScreen.Bounds);
            View.Add(_loadingOverlay);
        }

        private void HideOverlay()
        {
            _loadingOverlay.Hide();
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