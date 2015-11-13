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
	partial class ForgotPasswordViewController : UIViewController
	{
        #region Controls

        private UITextField _emailTextField;

        private UIButton _sendRecoveryButton;

        private UILabel _recoveryInfoLabel;
        private UILabel _emailValidationLabel;

        private UIActivityIndicatorView _activityIndicator;

        #endregion

        private ForgotPasswordViewModel _forgotPasswordViewModel;

        public ForgotPasswordViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            InitializeRecoveryInfoLabel();
            InitializeEMailTextField();
            InitializeEMailValidationLabel();
            InitializeSendRecoveryButton();
            InitializeActivityIndicator();

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
            _emailTextField.Text = _forgotPasswordViewModel.EMail = "freedomvoice.adm.267055@gmail.com";
        }

        private void InitializeViewModel()
        {
            _forgotPasswordViewModel = new ForgotPasswordViewModel(NavigationController, _activityIndicator);
        }

        private void OnSendButtonTouchUpInside(object sender, EventArgs args)
        {
            ProceedPasswordReset();
        }

        private bool OnEMailReturn(UITextField textField)
        {
            if (EMailValidate())
                ProceedPasswordReset();

            return true;
        }

	    private async void ProceedPasswordReset()
	    {
            _emailTextField.ResignFirstResponder();

            if (!_forgotPasswordViewModel.IsValid)
                EMailValidate();

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowNetworkUnreachableAlert(NavigationController);
                return;
            }

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            _sendRecoveryButton.Hidden = true;

            await _forgotPasswordViewModel.ForgotPasswordAsync();

            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; 
        }

        private void OnForgotPasswordSuccess(object sender, EventArgs e)
        {
            var alertController = UIAlertController.Create("Password reset email sent", "Please follow the instructions inside", UIAlertControllerStyle.Alert);
            alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, a => { ReturnToLogin(); }));
            PresentViewController(alertController, true, null);
        }

        private void OnForgotPasswordFailed(object sender, EventArgs args)
        {
            _sendRecoveryButton.Hidden = false;
            OnEMailValidationFailed();
        }

        private void ReturnToLogin()
        {
            NavigationController.PopViewController(true);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            UnsubscribeFromEvents();
        }

        #region Fields Validation

        private bool EMailValidate()
        {
            if (_forgotPasswordViewModel.Errors.Contains(ForgotPasswordViewModel.EMailError))
            {
                OnEMailValidationFailed();
                return false;
            }

            OnEMailValidationSuccess();
            return true;
        }

        private void OnEMailValidationSuccess()
        {
            _emailTextField.ResignFirstResponder();
            _emailTextField.Layer.BorderColor = Theme.LoginPageTextFieldBorderColor.ToCGColor();
            _emailValidationLabel.Hidden = true;
        }

        private void OnEMailValidationFailed()
        {
            _emailValidationLabel.Hidden = false;
            _emailTextField.Layer.BorderColor = Theme.InvalidTextFieldBorderColor.ToCGColor();
        }

        #endregion

        #region Events Subscription

        private void UnsubscribeFromEvents()
        {
            _forgotPasswordViewModel.OnBadRequestResponse -= OnForgotPasswordFailed;
            _forgotPasswordViewModel.OnUnauthorizedResponse -= OnForgotPasswordFailed;
            _forgotPasswordViewModel.OnSuccessResponse -= OnForgotPasswordSuccess;
        }

        private void SubscribeToEvents()
        {
            _forgotPasswordViewModel.OnBadRequestResponse += OnForgotPasswordFailed;
            _forgotPasswordViewModel.OnUnauthorizedResponse += OnForgotPasswordFailed;
            _forgotPasswordViewModel.OnSuccessResponse += OnForgotPasswordSuccess;
        }

        #endregion

        #region Controls Initialization

        private void InitializeRecoveryInfoLabel()
        {
            var labelFrame = new CGRect(15, 20, Theme.ScreenBounds.Width - 30, 40);
            _recoveryInfoLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                Text = $"Enter your email address below to get{Environment.NewLine}a recovery email to reset your password.",
                TextColor = Theme.BlackColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 2
            };
            _recoveryInfoLabel.Center = new CGPoint(View.Center.X, _recoveryInfoLabel.Center.Y);

            View.Add(_recoveryInfoLabel);
        }

        private void InitializeEMailTextField()
        {
            var textFieldFrame = new CGRect(15, _recoveryInfoLabel.Frame.Y + _recoveryInfoLabel.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44);
            _emailTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.BlackColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular),
                BorderStyle = UITextBorderStyle.RoundedRect,
                KeyboardType = UIKeyboardType.EmailAddress,
                AutocorrectionType = UITextAutocorrectionType.No,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AttributedPlaceholder = new NSAttributedString("Email", new UIStringAttributes { ForegroundColor = Theme.TextFieldHintColor })
            };
            _emailTextField.Layer.CornerRadius = 5;
            _emailTextField.Layer.BorderWidth = 1;
            _emailTextField.Layer.BorderColor = Theme.TextFieldBorderColor.ToCGColor();

            _emailTextField.SetDidChangeNotification(text => _forgotPasswordViewModel.EMail = text.Text);
            _emailTextField.EditingDidEnd += (s, args) => { EMailValidate(); };
            _emailTextField.ShouldReturn = OnEMailReturn;

            View.AddSubview(_emailTextField);
        }

        private void InitializeEMailValidationLabel()
        {
            var labelFrame = new CGRect(15, _emailTextField.Frame.Y + _emailTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 30, 14);
            _emailValidationLabel = new UILabel(labelFrame)
            {
                Text = "Please enter a valid email address.",
                TextColor = Theme.InvalidLabelColor,
                TextAlignment = UITextAlignment.Left,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                Hidden = true
            };

            View.Add(_emailValidationLabel);
        }

        private void InitializeSendRecoveryButton()
        {
            _sendRecoveryButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _emailTextField.Frame.Y + _emailTextField.Frame.Height + 27, Theme.ScreenBounds.Width - 30, 44),
                Font = UIFont.SystemFontOfSize(21, UIFontWeight.Medium),
                ClipsToBounds = true
            };
            _sendRecoveryButton.Layer.CornerRadius = 5;
            _sendRecoveryButton.Center = new CGPoint(View.Center.X, _sendRecoveryButton.Center.Y);
            _sendRecoveryButton.SetTitle("Send", UIControlState.Normal);
            _sendRecoveryButton.SetTitleColor(Theme.WhiteColor, UIControlState.Normal);
            _sendRecoveryButton.TouchUpInside += OnSendButtonTouchUpInside;

            View.AddSubview(_sendRecoveryButton);
        }

        private void InitializeActivityIndicator()
        {
            var frame = new CGRect(0, 0, 37, 37);
            _activityIndicator = new UIActivityIndicatorView(frame)
            {
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge,
                HidesWhenStopped = true,
                Center = _sendRecoveryButton.Center
            };

            View.AddSubview(_activityIndicator);
        }

        #endregion
    }
}