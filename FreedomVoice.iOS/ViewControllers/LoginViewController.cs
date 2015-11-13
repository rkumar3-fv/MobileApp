using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Extensions;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class LoginViewController : BaseViewController
    {
        #region Controls

	    private UIImageView _logoImage;

        private UITextField _usernameTextField;
        private UITextField _passwordTextField;

        private UIButton _loginButton;
        private UIButton _forgotPasswordButton;

        private UILabel _welcomeLabel;
        private UILabel _usernameValidationLabel;
        private UILabel _passwordValidationLabel;
        private UILabel _authorizationFailedLabel;

	    private UIActivityIndicatorView _activityIndicator;

        #endregion

        public event EventHandler OnLoginSuccess;

        private LoginViewModel _loginViewModel;

        public LoginViewController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
            NavigationItem.Title = "Login";
            View.BackgroundColor = Theme.LoginBackground;

            InitializeLogoImage();
            InitializeWelcomeLabel();
	        InitializeUsernameTextField();
	        InitializeUsernameValidationLabel();
            InitializePasswordTextField();
	        InitializePasswordValidationLabel();
	        InitializeLoginButton();
	        InitializeActivityIndicator();
	        InitializeAuthorizationFailedLabel();
            InitializeForgotPasswordButton();

            InitializeViewModel();

            UnsubscribeFromEvents();
            SubscribeToEvents();

            FillInLoginData();

            base.ViewDidLoad();
        }

	    /// <summary>
        /// Only for test purposes, will be removed later
        /// </summary>
	    private void FillInLoginData()
	    {
            _usernameTextField.Text = _loginViewModel.Username = "freedomvoice.adm.267055@gmail.com";
            _passwordTextField.Text = _loginViewModel.Password = "adm654654";
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationController.NavigationBar.Hidden = true;
            base.ViewWillAppear(animated);
        }

	    public override void ViewWillDisappear(bool animated)
	    {
            base.ViewWillDisappear(animated);
            NavigationController.NavigationBar.Hidden = false;
        }

        private void InitializeViewModel()
        {
            _loginViewModel = new LoginViewModel(NavigationController, _activityIndicator);
        }

        private void OnLoginButtonTouchUpInside(object sender, EventArgs args)
	    {
            ProceedLogin();
        }

        private void OnForgotPasswordTouchUpInside(object sender, EventArgs args)
        {
            _usernameValidationLabel.Hidden = true;
            _passwordValidationLabel.Hidden = true;
            _authorizationFailedLabel.Hidden = true;

            _usernameTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
            _passwordTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();

            var forgotPasswordController = AppDelegate.GetViewController<ForgotPasswordViewController>();
            NavigationController.PushViewController(forgotPasswordController, true);
        }

	    private bool OnUsernameReturn(UITextField textField)
	    {
	        if (UsernameValidate())
                _passwordTextField.BecomeFirstResponder();

            return true;
	    }

	    private bool OnPasswordReturn(UITextField textField)
	    {
            if (PasswordValidate() && _loginViewModel.IsValid)
                ProceedLogin();

            return true;
        }

	    private async void ProceedLogin()
	    {
	        _usernameTextField.ResignFirstResponder();
	        _passwordTextField.ResignFirstResponder();

	        if (!_loginViewModel.IsValid)
            {
                UsernameValidate();
                PasswordValidate();
                return;
            }

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowNetworkUnreachableAlert(NavigationController);
                return;
            }

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            _loginButton.Hidden = true;

            await _loginViewModel.LoginAsync();

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            _authorizationFailedLabel.Hidden = false;
            _loginButton.Hidden = false;

            OnUsernameValidationFailed();
            OnPasswordValidationFailed();
        }

        private void OnErrorConnection(object sender, EventArgs args)
        {
            _authorizationFailedLabel.Hidden = true;
            _loginButton.Hidden = false;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            UnsubscribeFromEvents();
        }

        protected override bool HandlesKeyboardNotifications => true;

	    protected override void OnKeyboardChanged(bool visible, nfloat height)
	    {
            AdjustLogoImage(visible);
            AdjustWelcomeLabel(visible);
            AdjustUsernameTextField(visible);
            AdjustUsernameValidationLabel();
            AdjustPasswordTextField();
            AdjustPasswordValidationLabel();
            AdjustLoginButton();
            AdjustActivityIndicator();
            AdjustAuthorizationFailedLabel();
            AdjustForgotPasswordButton(height, visible);
        }

        #region Events Subscription

        private void UnsubscribeFromEvents()
	    {
            _loginViewModel.OnSuccessResponse -= OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse -= OnLoginFailed;
            _loginViewModel.OnErrorConnectionResponse -= OnErrorConnection;
            _loginViewModel.OnBadRequestResponse -= OnLoginFailed;
        }

        private void SubscribeToEvents()
        {
            _loginViewModel.OnSuccessResponse += OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse += OnLoginFailed;
            _loginViewModel.OnErrorConnectionResponse += OnErrorConnection;
            _loginViewModel.OnBadRequestResponse += OnLoginFailed;
        }

        #endregion

        #region Fields Validation

        private bool UsernameValidate()
        {
            if (_loginViewModel.Errors.Contains(LoginViewModel.UsernameError))
            {
                OnUsernameValidationFailed();
                return false;
            }

            OnUsernameValidationSuccess();
            return true;
        }

        private bool PasswordValidate()
        {
            if (_loginViewModel.Errors.Contains(LoginViewModel.PasswordError))
            {
                OnPasswordValidationFailed();
                return false;
            }

            OnPasswordValidationSuccess();
            return true;
        }

        private void OnUsernameValidationSuccess()
        {
            _usernameTextField.ResignFirstResponder();
            _usernameValidationLabel.Hidden = true;
            _usernameTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
        }

        private void OnPasswordValidationSuccess()
        {
            _passwordTextField.ResignFirstResponder();
            _passwordValidationLabel.Hidden = true;
            _passwordTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
        }

        private void OnUsernameValidationFailed()
        {
            _usernameValidationLabel.Hidden = false;
            _usernameTextField.Layer.BorderColor = Theme.InvalidTextFieldBorderColor.ToCGColor();
        }

        private void OnPasswordValidationFailed()
        {
            _passwordValidationLabel.Hidden = false;
            _passwordTextField.Layer.BorderColor = Theme.InvalidTextFieldBorderColor.ToCGColor();
        }

        #endregion

        #region Controls Initialization

        private void InitializeLogoImage()
	    {
            _logoImage = new UIImageView(UIImage.FromFile("logo_freedomvoice_white.png"));
            _logoImage.Frame = new CGRect(0, Theme.StatusBarHeight + 35, _logoImage.Image.CGImage.Width / 2, _logoImage.Image.CGImage.Height / 2);
            _logoImage.Center = new CGPoint(View.Center.X, _logoImage.Center.Y);

            View.AddSubview(_logoImage);
        }

        private void InitializeWelcomeLabel()
        {
            var labelFrame = new CGRect(15, Theme.StatusBarHeight + 136, Theme.ScreenBounds.Width - 30, 30);
            _welcomeLabel = new UILabel(labelFrame)
            {
                Text = "Welcome",
                TextColor = Theme.WhiteColor,
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.SystemFontOfSize(36, UIFontWeight.Thin)
            };
            _welcomeLabel.Center = new CGPoint(View.Center.X, _welcomeLabel.Center.Y);

            View.Add(_welcomeLabel);
        }

        private void InitializeUsernameTextField()
        {
            var textFieldFrame = new CGRect(15, _welcomeLabel.Frame.Y + _welcomeLabel.Frame.Height + 30, Theme.ScreenBounds.Width - 30, 44);
            _usernameTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.LoginPageTextFieldTextColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular),
                BorderStyle = UITextBorderStyle.RoundedRect,
                KeyboardType = UIKeyboardType.EmailAddress,
                AutocorrectionType = UITextAutocorrectionType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AttributedPlaceholder = new NSAttributedString("Username or Email", new UIStringAttributes {ForegroundColor = Theme.LoginPageTextFieldHintColor})
            };
            _usernameTextField.Layer.CornerRadius = 5;
            _usernameTextField.Layer.BorderWidth = 1;
            _usernameTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();

            _usernameTextField.SetDidChangeNotification(text => _loginViewModel.Username = text.Text);
            _usernameTextField.EditingDidEnd += (s, args) => { UsernameValidate(); };
            _usernameTextField.ShouldReturn = OnUsernameReturn;

            View.AddSubview(_usernameTextField);
        }

        private void InitializeUsernameValidationLabel()
        {
            var labelFrame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 30, 14);
            _usernameValidationLabel = new UILabel(labelFrame)
            {
                Text = "Please enter a valid email address.",
                TextColor = Theme.InvalidLabelColor,
                TextAlignment = UITextAlignment.Left,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                Hidden = true
            };

            View.Add(_usernameValidationLabel);
        }

        private void InitializePasswordTextField()
        {
            var textFieldFrame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44);
            _passwordTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.LoginPageTextFieldTextColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular),
                BorderStyle = UITextBorderStyle.RoundedRect,
                AutocorrectionType = UITextAutocorrectionType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                SecureTextEntry = true,
                AttributedPlaceholder = new NSAttributedString("Password", new UIStringAttributes { ForegroundColor = Theme.LoginPageTextFieldHintColor })
            };
            _passwordTextField.Layer.CornerRadius = 5;
            _passwordTextField.Layer.BorderWidth = 1;
            _passwordTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();

            _passwordTextField.SetDidChangeNotification(text => _loginViewModel.Password = text.Text);
            _passwordTextField.EditingDidEnd += (s, args) => { PasswordValidate(); };
            _passwordTextField.ShouldReturn = OnPasswordReturn;

            View.AddSubview(_passwordTextField);
        }

	    private void InitializePasswordValidationLabel()
        {
            var labelFrame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 30, 14);
            _passwordValidationLabel = new UILabel(labelFrame)
            {
                Text = "Please enter your FreedomVoice password.",
                TextColor = Theme.InvalidLabelColor,
                TextAlignment = UITextAlignment.Left,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                Hidden = true
            };

            View.Add(_passwordValidationLabel);
        }

        private void InitializeLoginButton()
        {
            _loginButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44),
                Font = UIFont.SystemFontOfSize(21, UIFontWeight.Medium),
                ClipsToBounds = true
            };
            _loginButton.Layer.CornerRadius = 5;
            _loginButton.Center = new CGPoint(View.Center.X, _loginButton.Center.Y);
            _loginButton.SetTitle("Log in", UIControlState.Normal);
            _loginButton.SetTitleColor(Theme.WhiteColor, UIControlState.Normal);
            _loginButton.TouchUpInside += OnLoginButtonTouchUpInside;

            View.AddSubview(_loginButton);
        }

	    private void InitializeActivityIndicator()
	    {
	        var frame = new CGRect(0, 0, 37, 37);
	        _activityIndicator = new UIActivityIndicatorView(frame)
	        {
	            ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge,
                HidesWhenStopped = true,
                Center = _loginButton.Center
	        };

            View.AddSubview(_activityIndicator);
        }

        private void InitializeAuthorizationFailedLabel()
        {
            var labelFrame = new CGRect(15, _loginButton.Frame.Y + _loginButton.Frame.Height + 10, Theme.ScreenBounds.Width - 30, 14);
            _authorizationFailedLabel = new UILabel(labelFrame)
            {
                Text = "Incorrect email or password.",
                TextColor = Theme.InvalidLabelColor,
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                Hidden = true
            };
            _authorizationFailedLabel.Center = new CGPoint(View.Center.X, _authorizationFailedLabel.Center.Y);

            View.Add(_authorizationFailedLabel);
        }

        private void InitializeForgotPasswordButton()
        {
            _forgotPasswordButton = new UIButton(UIButtonType.System);
            _forgotPasswordButton.SetTitle("Forgot your password?", UIControlState.Normal);
            _forgotPasswordButton.SetTitleColor(Theme.ForgotPasswordButtonColor, UIControlState.Normal);
            _forgotPasswordButton.Frame = new CGRect(0, Theme.ScreenBounds.Height - 35, 185, 13);
            _forgotPasswordButton.Font = UIFont.SystemFontOfSize(17, UIFontWeight.Thin);
            _forgotPasswordButton.Center = new CGPoint(View.Center.X, _forgotPasswordButton.Center.Y);
            _forgotPasswordButton.TouchUpInside += OnForgotPasswordTouchUpInside;

            View.AddSubview(_forgotPasswordButton);
        }

        #endregion

        #region Controls Adjust

        private void AdjustLogoImage(bool keyboardVisible = false)
        {
            _logoImage.Image = keyboardVisible ? UIImage.FromFile("logo_freedomvoice_small_white.png") : UIImage.FromFile("logo_freedomvoice_white.png");
            _logoImage.Frame = new CGRect(0, Theme.StatusBarHeight + (keyboardVisible ? 10 : 35), _logoImage.Image.CGImage.Width / 2, _logoImage.Image.CGImage.Height / 2);
            _logoImage.Center = new CGPoint(View.Center.X, _logoImage.Center.Y);
        }

        private void AdjustWelcomeLabel(bool keyboardVisible = false)
        {
            _welcomeLabel.Font = UIFont.SystemFontOfSize((keyboardVisible ? 24 : 36), UIFontWeight.Thin);
            _welcomeLabel.Frame = new CGRect(15, Theme.StatusBarHeight + (keyboardVisible ? 43 : 135), Theme.ScreenBounds.Width - 30, (keyboardVisible ? 18 : 30));
            _welcomeLabel.Center = new CGPoint(View.Center.X, _welcomeLabel.Center.Y);
        }

	    private void AdjustUsernameTextField(bool keyboardVisible = false)
	    {
	        _usernameTextField.Frame = new CGRect(15, _welcomeLabel.Frame.Y + _welcomeLabel.Frame.Height + (keyboardVisible ? 10 : 30), Theme.ScreenBounds.Width - 30, 44);
	    }

        private void AdjustUsernameValidationLabel()
        {
            _usernameValidationLabel.Frame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 30, 14);
        }

        private void AdjustPasswordTextField()
        {
            _passwordTextField.Frame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44);
        }

        private void AdjustPasswordValidationLabel()
        {
            _passwordValidationLabel.Frame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 30, 14);
        }

        private void AdjustLoginButton()
        {
            _loginButton.Frame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44);
        }

        private void AdjustActivityIndicator()
        {
            _activityIndicator.Center = _loginButton.Center;
        }

        private void AdjustAuthorizationFailedLabel()
        {
            _authorizationFailedLabel.Frame = new CGRect(15, _loginButton.Frame.Y + _loginButton.Frame.Height + 10, Theme.ScreenBounds.Width - 30, 14);
        }

        private void AdjustForgotPasswordButton(nfloat keyboardHeight, bool keyboardVisible = false)
        {
            _forgotPasswordButton.Frame = new CGRect(0, Theme.ScreenBounds.Height - (keyboardVisible ? keyboardHeight + 30 : 35), 185, 13);
            _forgotPasswordButton.Center = new CGPoint(View.Center.X, _forgotPasswordButton.Center.Y);
        }

        #endregion
    }
}