using System;
using CoreGraphics;
using FreedomVoice.Core;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class ForgotPasswordViewController : UIViewController
	{
		public ForgotPasswordViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NavigationController.SetDefaultNavigationBarStyle();

            EmailTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            SendButton.Layer.CornerRadius = 5;
            SendButton.ClipsToBounds = true;
        }

        partial void SendButton_TouchUpInside(UIButton sender)
        {
            var emailAddress = EmailTextField.Text.Trim();
            if (Validation.IsValidEmail(emailAddress))
            {
                ProceedPasswordReset(emailAddress);
            }
            else
            {
                InvokeOnMainThread(() => { EmailValidationLabel.Hidden = false; EmailTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1); });
                EmailTextField.BecomeFirstResponder();
            }
        }

	    async private void ProceedPasswordReset(string emailAddress)
	    {
            var passwordResetResult = await ApiHelper.PasswordReset(emailAddress);
            switch (passwordResetResult.Code)
            {
                case ErrorCodes.Ok:
                    ShowAlert("Password reset email sent", "Please follow the instructions inside", "Ok");
                    break;
                case ErrorCodes.ConnectionLost:
                    new UIAlertView("Password reset error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
                    break;
                default:
                    InvokeOnMainThread(() => { EmailValidationLabel.Hidden = false; EmailTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1); });
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
            NavigationController.PopViewController(false);
        }
    }
}