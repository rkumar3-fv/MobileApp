using System;
using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class LoginViewController : BaseViewController
    {
        readonly LoginViewModel _loginViewModel;

        public event EventHandler OnLoginSuccess;

	    public LoginViewController(IntPtr handle) : base(handle)
	    {
            _loginViewModel = ServiceContainer.Resolve<LoginViewModel>();

            _loginViewModel.IsBusyChanged += OnIsBusyChanged;
            _loginViewModel.IsValidChanged += OnIsValidChanged;
        }

	    public override void ViewDidLoad()
	    {
	        base.ViewDidLoad();

	        Title = "Login";

            _loginViewModel.OnSuccessResponse += OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse += OnLoginFailed;
            _loginViewModel.OnBadRequestResponse += OnLoginFailed;

            UsernameTextField.SetDidChangeNotification(text => _loginViewModel.Username = text.Text);
	        UsernameTextField.ShouldReturn = _ => { OnUsernameReturn(); return false; };

            PasswordTextField.SetDidChangeNotification(text => _loginViewModel.Password = text.Text);
            PasswordTextField.ShouldReturn = _ => {
                if (_loginViewModel.IsValid) ProceedLogin();
                else
                {
                    OnUsernameReturn();
                    OnPasswordReturn();
                }
                return false;
            };

            UsernameTextField.Text = _loginViewModel.Username = "freedomvoice.user1.267055@gmail.com";
            PasswordTextField.Text = _loginViewModel.Password = "user1654654";

            View.AddLinearGradientToView(UIColor.FromRGB(0, 59, 108), UIColor.FromRGB(218, 242, 246));

            UsernameTextField.BorderStyle = PasswordTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            LoginButton.Layer.CornerRadius = 5;
            LoginButton.ClipsToBounds = true;
        }

        partial void LoginButton_TouchUpInside(UIButton sender)
	    {
            ProceedLogin();
        }

        partial void ForgotPassword_TouchUpInside(UIButton sender)
        {
            var forgotPasswordController = AppDelegate.GetViewController<ForgotPasswordViewController>();
            NavigationController.PushViewController(forgotPasswordController, true);
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!IsViewLoaded)
                return;

            View.UserInteractionEnabled = ActivityIndicator.Hidden = !_loginViewModel.IsBusy;
            LoginButton.Hidden = _loginViewModel.IsBusy;
        }

        private void OnIsValidChanged(object sender, EventArgs e)
        {
            if (IsViewLoaded)
                LoginButton.Enabled = _loginViewModel.IsValid;
        }

	    private void OnUsernameReturn()
	    {
	        if (_loginViewModel.Errors.Contains(LoginViewModel.UsernameError))
	            OnUsernameValidationFailed();
            else
                PasswordTextField.BecomeFirstResponder();
        }

        private void OnPasswordReturn()
        {
            if (!_loginViewModel.Errors.Contains(LoginViewModel.PasswordError))
                return;

            PasswordValidationLabel.Hidden = false;
            PasswordTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            PasswordTextField.BecomeFirstResponder();
        }

        private async void ProceedLogin()
        {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            UsernameTextField.ResignFirstResponder();
            PasswordTextField.ResignFirstResponder();

            _loginViewModel.IsBusy = true;
            await _loginViewModel.LoginAsync();
            _loginViewModel.IsBusy = false;

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            UsernameValidationLabel.Hidden = false;
            UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            UsernameTextField.BecomeFirstResponder();
        }

        private void OnUsernameValidationFailed()
	    {
            UsernameValidationLabel.Hidden = false;
            UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            UsernameTextField.BecomeFirstResponder();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            base.ViewWillAppear(animated);

            //UsernameTextField.Text = string.Empty;
            UsernameValidationLabel.Hidden = true;

            //PasswordTextField.Text = string.Empty;
            PasswordValidationLabel.Hidden = true;

            NavigationController.NavigationBar.Hidden = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillAppear(animated);
            NavigationController.NavigationBar.Hidden = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _loginViewModel.IsBusyChanged -= OnIsBusyChanged;
            _loginViewModel.IsValidChanged -= OnIsValidChanged;
            _loginViewModel.OnSuccessResponse -= OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse -= OnLoginFailed;
            _loginViewModel.OnBadRequestResponse -= OnLoginFailed;
        }

        protected override void OnKeyboardChanged(bool visible, nfloat height)
        {
            //We "center" the popup when the keyboard appears/disappears
            var frame = View.Frame;

            if (visible)
                frame.Y -= height / 2f;
            else
                frame.Y += height / 2f;

            View.Frame = frame;
        }
    }
}