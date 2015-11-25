using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Extensions;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using GoogleAnalytics.iOS;
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
        private UILabel _validationFailedLabel;

        #endregion

        public event EventHandler OnLoginSuccess;

        private LoginViewModel _loginViewModel;

	    public LoginViewController(IntPtr handle) : base(handle)
	    {
            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Login Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

	    public override void ViewDidLoad()
	    {
            NavigationItem.Title = "Login";
            View.BackgroundColor = Theme.LoginBackground;

            InitializeLogoImage();
            InitializeWelcomeLabel();
	        InitializeUsernameTextField();
            InitializePasswordTextField();
	        InitializeLoginButton();
	        InitializeValidationFailedLabel();
            InitializeForgotPasswordButton();

            InitializeViewModel();
            InitializeActivityIndicator();

            UnsubscribeFromEvents();
            SubscribeToEvents();
#if DEBUG
            FillInLoginData();
#endif
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
            View.UserInteractionEnabled = true;
            _loginButton.Hidden = false;

            base.ViewWillAppear(animated);
        }

	    public override void ViewWillDisappear(bool animated)
	    {
            NavigationController.NavigationBar.Hidden = false;
            base.ViewWillDisappear(animated);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            View.EndEditing(true);
        }

        private void InitializeViewModel()
        {
            _loginViewModel = new LoginViewModel(NavigationController);
        }

        private void OnLoginButtonTouchUpInside(object sender, EventArgs args)
	    {
            ProceedLogin();
        }

        private void OnForgotPasswordTouchUpInside(object sender, EventArgs args)
        {
            _validationFailedLabel.Hidden = true;

            _usernameTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
            _passwordTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();

            var forgotPasswordController = AppDelegate.GetViewController<ForgotPasswordViewController>();
            forgotPasswordController.EmailAddress = _usernameTextField.Text;
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
                Appearance.ShowOkAlertWithMessage(NavigationController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

	        DisableUserInteraction();

            await _loginViewModel.LoginAsync();
        }

        private void OnLoginFailed(object sender, EventArgs args)
        {
            _validationFailedLabel.Text = "Incorrect email or password.";
            _validationFailedLabel.Hidden = false;

            EnableUserInteraction();
        }

        private void OnLoginBadRequest(object sender, EventArgs args)
        {
            _validationFailedLabel.Text = "Server is unavailable.";
            _validationFailedLabel.Hidden = false;

            EnableUserInteraction();
        }

        private void OnLoginUnknownError(object sender, EventArgs args)
        {
            _validationFailedLabel.Text = "Internal server error.";
            _validationFailedLabel.Hidden = false;

            EnableUserInteraction();
        }

	    private void EnableUserInteraction()
	    {
            View.UserInteractionEnabled = true;
            _loginButton.Hidden = false;
        }

        private void DisableUserInteraction()
        {
            View.UserInteractionEnabled = false;
            _loginButton.Hidden = true;
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
            AdjustPasswordTextField();
            AdjustLoginButton();
            AdjustActivityIndicator();
            AdjustValidationFailedLabel();
            AdjustForgotPasswordButton(height, visible);
        }

        #region Events Subscription

        private void UnsubscribeFromEvents()
	    {
            _loginViewModel.OnSuccessResponse -= OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse -= OnLoginFailed;
            _loginViewModel.OnBadRequestResponse -= OnLoginBadRequest;
            _loginViewModel.OnInternalErrorResponse -= OnLoginUnknownError;
        }

        private void SubscribeToEvents()
        {
            _loginViewModel.OnSuccessResponse += OnLoginSuccess;
            _loginViewModel.OnUnauthorizedResponse += OnLoginFailed;
            _loginViewModel.OnBadRequestResponse += OnLoginBadRequest;
            _loginViewModel.OnInternalErrorResponse += OnLoginUnknownError;
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
            if (_loginViewModel.Errors.Contains(LoginViewModel.UsernameError)) return false;

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
            _usernameTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();

            _validationFailedLabel.Hidden = true;
        }

        private void OnPasswordValidationSuccess()
        {
            _passwordTextField.ResignFirstResponder();

            _passwordTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
            _validationFailedLabel.Hidden = true;
        }

        private void OnUsernameValidationFailed()
        {
            _usernameTextField.Layer.BorderColor = Theme.InvalidTextFieldBorderColor.ToCGColor();
            _validationFailedLabel.Text = "Please enter a valid email address.";
            _validationFailedLabel.Hidden = false;
        }

        private void OnPasswordValidationFailed()
        {
            _passwordTextField.Layer.BorderColor = Theme.InvalidTextFieldBorderColor.ToCGColor();
            _validationFailedLabel.Text = "Please enter your FreedomVoice password.";
            _validationFailedLabel.Hidden = false;
        }

        #endregion

        #region Controls Initialization

        private void InitializeLogoImage()
	    {
            _logoImage = new UIImageView(UIImage.FromFile("logo_freedomvoice_white.png")) { Frame = new CGRect(0, Theme.StatusBarHeight + 35, Theme.LogoImageWidth(), Theme.LogoImageHeight()) };
            _logoImage.Center = new CGPoint(View.Center.X, _logoImage.Center.Y);

            View.AddSubview(_logoImage);
        }

        private void InitializeWelcomeLabel()
        {
            var labelFrame = new CGRect(15, _logoImage.Frame.Y + _logoImage.Frame.Height + Theme.WelcomeLabelTopPadding(), Theme.ScreenBounds.Width - 30, Theme.WelcomeLabelHeight());
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
            var textFieldFrame = new CGRect(15, _welcomeLabel.Frame.Y + _welcomeLabel.Frame.Height + Theme.UsernameTextFieldTopPadding(), Theme.ScreenBounds.Width - 30, 44);
            _usernameTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.LoginPageTextFieldTextColor,
                TintColor = Theme.WhiteColor,
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

        private void InitializePasswordTextField()
        {
            var textFieldFrame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + Theme.PasswordTextFieldPadding, Theme.ScreenBounds.Width - 30, 44);
            _passwordTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.LoginPageTextFieldTextColor,
                TintColor = Theme.WhiteColor,
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

        private void InitializeLoginButton()
        {
            _loginButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + Theme.PasswordTextFieldPadding, Theme.ScreenBounds.Width - 30, 44),
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

        private void InitializeValidationFailedLabel()
        {
            var labelFrame = new CGRect(15, _loginButton.Frame.Y + _loginButton.Frame.Height + Theme.LoginValidationLabelTopPadding, Theme.ScreenBounds.Width - 30, 20);
            _validationFailedLabel = new UILabel(labelFrame)
            {
                BackgroundColor = Theme.InvalidLabelBackgroundColor,
                TextColor = Theme.LoginInvalidLabelColor,
                TextAlignment = UITextAlignment.Center,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                ClipsToBounds = true,
                Hidden = true
            };
            _validationFailedLabel.Layer.CornerRadius = 5;
            _validationFailedLabel.Center = new CGPoint(View.Center.X, _validationFailedLabel.Center.Y);

            View.Add(_validationFailedLabel);
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

        private void InitializeActivityIndicator()
        {
            _loginViewModel.ActivityIndicatorCenter = _loginButton.Center;
        }

        #endregion

        #region Controls Adjust

        private void AdjustLogoImage(bool keyboardVisible = false)
        {
            _logoImage.Image = Theme.LoginLogoImage(keyboardVisible);
            _logoImage.Frame = new CGRect(0, Theme.StatusBarHeight + Theme.LogoImageTopPadding(keyboardVisible), Theme.LogoImageWidth(keyboardVisible), Theme.LogoImageHeight(keyboardVisible));
            _logoImage.Center = new CGPoint(View.Center.X, _logoImage.Center.Y);
        }

        private void AdjustWelcomeLabel(bool keyboardVisible = false)
        {
            _welcomeLabel.Font = Theme.WelcomeLabelFont(keyboardVisible);
            _welcomeLabel.Frame = new CGRect(15, _logoImage.Frame.Y + _logoImage.Frame.Height + Theme.WelcomeLabelTopPadding(keyboardVisible), Theme.ScreenBounds.Width - 30, Theme.WelcomeLabelHeight(keyboardVisible));
            _welcomeLabel.Center = new CGPoint(View.Center.X, _welcomeLabel.Center.Y);
        }

	    private void AdjustUsernameTextField(bool keyboardVisible = false)
	    {
	        _usernameTextField.Frame = new CGRect(15, _welcomeLabel.Frame.Y + _welcomeLabel.Frame.Height + Theme.UsernameTextFieldTopPadding(keyboardVisible), Theme.ScreenBounds.Width - 30, 44);
	    }

        private void AdjustPasswordTextField()
        {
            _passwordTextField.Frame = new CGRect(15, _usernameTextField.Frame.Y + _usernameTextField.Frame.Height + Theme.PasswordTextFieldPadding, Theme.ScreenBounds.Width - 30, 44);
        }

        private void AdjustLoginButton()
        {
            _loginButton.Frame = new CGRect(15, _passwordTextField.Frame.Y + _passwordTextField.Frame.Height + Theme.PasswordTextFieldPadding, Theme.ScreenBounds.Width - 30, 44);
        }

        private void AdjustActivityIndicator()
        {
            _loginViewModel.ActivityIndicatorCenter = _loginButton.Center;
        }

        private void AdjustValidationFailedLabel()
        {
            _validationFailedLabel.Frame = new CGRect(15, _loginButton.Frame.Y + _loginButton.Frame.Height + Theme.LoginValidationLabelTopPadding, Theme.ScreenBounds.Width - 30, 20);
        }

        private void AdjustForgotPasswordButton(nfloat keyboardHeight, bool keyboardVisible = false)
        {
            _forgotPasswordButton.Frame = new CGRect(0, Theme.ScreenBounds.Height - (keyboardVisible ? keyboardHeight + 30 : 35), 185, 13);
            _forgotPasswordButton.Center = new CGPoint(View.Center.X, _forgotPasswordButton.Center.Y);
        }

        #endregion
    }
}