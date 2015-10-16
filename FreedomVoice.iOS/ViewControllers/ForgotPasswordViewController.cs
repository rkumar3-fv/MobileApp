using System;
using System.Threading.Tasks;
using CoreGraphics;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class ForgotPasswordViewController : UIViewController
	{
        readonly ForgotPasswordViewModel _forgotPasswordViewModel;

        public ForgotPasswordViewController(IntPtr handle) : base(handle)
	    {
            _forgotPasswordViewModel = ServiceContainer.Resolve<ForgotPasswordViewModel>();

            _forgotPasswordViewModel.IsBusyChanged += OnIsBusyChanged;
            _forgotPasswordViewModel.IsValidChanged += OnIsValidChanged;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            EmailTextField.SetDidChangeNotification(text => _forgotPasswordViewModel.EMail = text.Text);
            EmailTextField.ShouldReturn = _ => {
                if (_forgotPasswordViewModel.IsValid) ProceedPasswordReset();
                else OnEMailReturn();
                return false;
            };

            EmailTextField.Text = _forgotPasswordViewModel.EMail = "freedomvoice.user1.267055@gmail.com";

            EmailTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            SendButton.Layer.CornerRadius = 5;
            SendButton.ClipsToBounds = true;
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!IsViewLoaded)
                return;

            View.UserInteractionEnabled = ActivityIndicator.Hidden = !_forgotPasswordViewModel.IsBusy;
            SendButton.Hidden = _forgotPasswordViewModel.IsBusy;
        }

        private void OnIsValidChanged(object sender, EventArgs e)
        {
            if (IsViewLoaded)
                SendButton.Enabled = _forgotPasswordViewModel.IsValid;
        }

        private void OnEMailReturn()
        {
            if (!_forgotPasswordViewModel.Errors.Contains(ForgotPasswordViewModel.EMailError)) return;

            EmailValidationLabel.Hidden = false;
            EmailTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            EmailTextField.BecomeFirstResponder();
        }

        partial void SendButton_TouchUpInside(UIButton sender)
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            ProceedPasswordReset();

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

	    async private void ProceedPasswordReset()
	    {
            EmailTextField.ResignFirstResponder();

            await _forgotPasswordViewModel.ForgotPasswordAsync().ContinueWith(_ => BeginInvokeOnMainThread(() => { OnProceedPasswordResetResponse(_); }));
        }

        private void OnProceedPasswordResetResponse(Task<BaseResult<string>> _)
        {
            switch (_.Result.Code)
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
            NavigationController.PopViewController(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _forgotPasswordViewModel.IsBusyChanged -= OnIsBusyChanged;
            _forgotPasswordViewModel.IsValidChanged -= OnIsValidChanged;
        }
    }
}