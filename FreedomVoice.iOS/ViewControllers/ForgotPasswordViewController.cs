using System;
using CoreGraphics;
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

            _forgotPasswordViewModel.OnBadRequestResponse += OnEMailValidationFailed;
            _forgotPasswordViewModel.OnUnauthorizedResponse += OnEMailValidationFailed;
            _forgotPasswordViewModel.OnSuccessResponse += OnForgotPasswordSuccess;

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
            if (!_forgotPasswordViewModel.Errors.Contains(ForgotPasswordViewModel.EMailError))
                return;

            OnEMailValidationFailed(null, EventArgs.Empty);
        }

        private void OnEMailValidationFailed(object sender, EventArgs e)
        {
            EmailValidationLabel.Hidden = false;
            EmailTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            EmailTextField.BecomeFirstResponder();
        }

        private void OnForgotPasswordSuccess(object sender, EventArgs e)
        {
            ShowAlert("Password reset email sent", "Please follow the instructions inside", "Ok");
        }

        partial void SendButton_TouchUpInside(UIButton sender)
        {
            ProceedPasswordReset();
        }

	    private async void ProceedPasswordReset()
	    {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            EmailTextField.ResignFirstResponder();

	        _forgotPasswordViewModel.IsBusy = true;
            await _forgotPasswordViewModel.ForgotPasswordAsync();
            _forgotPasswordViewModel.IsBusy = false;

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        private void ShowAlert(string title, string message, params string[] buttons)
        {
            var alertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create(buttons[0], UIAlertActionStyle.Cancel, a => { ReturnToLogin(); }));
            PresentViewController(alertController, true, null);
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
            _forgotPasswordViewModel.OnBadRequestResponse -= OnEMailValidationFailed;
            _forgotPasswordViewModel.OnUnauthorizedResponse -= OnEMailValidationFailed;
            _forgotPasswordViewModel.OnSuccessResponse -= OnForgotPasswordSuccess;
        }
    }
}