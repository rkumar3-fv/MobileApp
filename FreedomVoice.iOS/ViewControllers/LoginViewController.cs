using System;
using System.Threading.Tasks;
using CoreGraphics;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.Core.Entities.Enums;
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

            View.AddLinearGradientToView(new UIColor(0, 0.231f, 0.424f, 1), new UIColor(0.855f, 0.949f, 0.965f, 1));

            UsernameTextField.BorderStyle = PasswordTextField.BorderStyle = UITextBorderStyle.RoundedRect;

            LoginButton.Layer.CornerRadius = 5;
            LoginButton.ClipsToBounds = true;
        }

        partial void LoginButton_TouchUpInside(UIButton sender)
	    {
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            ProceedLogin();

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
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
	        {
                UsernameValidationLabel.Hidden = false;
                UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
                UsernameTextField.BecomeFirstResponder();
            }
            else
                PasswordTextField.BecomeFirstResponder();
        }

        private void OnPasswordReturn()
        {
            if (!_loginViewModel.Errors.Contains(LoginViewModel.PasswordError)) return;

            PasswordValidationLabel.Hidden = false;
            PasswordTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
            PasswordTextField.BecomeFirstResponder();
        }

        private async void ProceedLogin()
        {
            UsernameTextField.ResignFirstResponder();
            PasswordTextField.ResignFirstResponder();

            await _loginViewModel.LoginAsync().ContinueWith(_ => BeginInvokeOnMainThread(() => { OnProceedLoginResponse(_); }));
        }

	    private void OnProceedLoginResponse(Task<BaseResult<string>> _)
	    {
	        switch (_.Result.Code)
	        {
	            case ErrorCodes.Ok:
	                OnLoginSuccess?.Invoke(null, EventArgs.Empty);
	                break;
	            case ErrorCodes.ConnectionLost:
	                new UIAlertView("Login Error", "Service is unreachable. Please try again later.", null, "OK", null).Show();
	                break;
	            case ErrorCodes.Unauthorized:
	            case ErrorCodes.BadRequest:
	                UsernameValidationLabel.Hidden = false;
	                UsernameTextField.Layer.BorderColor = new CGColor(0.996f, 0.788f, 0.373f, 1);
	                UsernameTextField.BecomeFirstResponder();
	                break;
	        }
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
        }

        //protected override void OnKeyboardChanged(bool visible, nfloat height)
        //{
        //    //We "center" the popup when the keyboard appears/disappears
        //    var frame = container.Frame;

        //    if (visible)
        //        frame.Y -= height / 2f;
        //    else
        //        frame.Y += height / 2f;
        //    container.Frame = frame;
        //}
    }
}