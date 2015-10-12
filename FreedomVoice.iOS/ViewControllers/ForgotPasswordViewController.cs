using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class ForgotPasswordViewController : UIViewController
	{
		public ForgotPasswordViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.NavigationBar.TintColor = UIColor.White;
            NavigationController.NavigationBar.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            NavigationController.NavigationBar.BarTintColor = new UIColor(0.016f, 0.588f, 0.816f, 1);

            EmailTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            SendButton.Layer.CornerRadius = 5;
            SendButton.ClipsToBounds = true;
        }

        partial void SendButton_TouchUpInside(UIButton sender)
	    {
	        ProceedPasswordReset();
        }

	    async private void ProceedPasswordReset()
	    {
            var passwordResetResult = await ApiHelper.PasswordReset(EmailTextField.Text.Trim());
            switch (passwordResetResult.Code)
            {
                case ErrorCodes.Ok:
                    ShowAlert("Password reset email sent", "Please follow the instructions inside", "Ok");
                    break;
                case ErrorCodes.ConnectionLost:
                    new UIAlertView("Password reset error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
                    break;
                default:
                    InvokeOnMainThread(() => {
                        EmailValidationLabel.Text = "Please enter a valid email address.";
                        EmailValidationLabel.Hidden = false;
                        EmailTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
                        EmailTextField.Layer.BorderWidth = 1;
                        EmailTextField.Layer.CornerRadius = 5;
                    });
                    EmailTextField.BecomeFirstResponder();
                    break;
            }
        }

        private void ShowAlert(string title, string message, params string[] buttons)
        {
            var alert = new UIAlertView { Title = title, Message = message };
            alert.AddButton(buttons[0]);

            alert.Clicked += (s, e) => ReturnToLogin();
            alert.Show();
        }

        private void ReturnToLogin()
        {
            DismissViewController(false, null);
        }
    }
}