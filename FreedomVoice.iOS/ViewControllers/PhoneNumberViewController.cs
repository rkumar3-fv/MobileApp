using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class PhoneNumberViewController : BaseViewController
	{
        protected override string PageName => "Phone Number Screen";

        #region Controls

        private UIButton _continuedButton;

        private UILabel _firstLabel;
        private UILabel _secondLabel;
        private UILabel _plusOneLabel;

        private UILabel _phoneValidationLabel;

        private UITextField _phoneNumberTextField;

        #endregion

        public Account SelectedAccount { private get; set; }
        public UIViewController ParentController { private get; set; }

	    public PhoneNumberViewController(IntPtr handle) : base(handle) { }

	    public override void ViewDidLoad()
	    {
            Title = "Your Mobile Phone Number";

            InitializeFirstLabel();
            InitializeSecondLabel();
            InitializePhoneNumberTextField();
            InitializePlusOneLabel();
            InitializeContinueButton();
            InitializePhoneValidationLabel();

            NavigationItem.SetRightBarButtonItem(Appearance.GetPlainBarButton("Skip", OnSkipButtonTouchUpInside), false);

            base.ViewDidLoad();
	    }

        private void OnArrowButtonTouchUpInside(object sender, EventArgs args)
        {
            ProceedPhoneNumberSave();
        }

        private void ProceedPhoneNumberSave()
        {
            var phoneNumber = _phoneNumberTextField.Text;

            _phoneNumberTextField.ResignFirstResponder();

            if (!PhoneNumberValidate(phoneNumber))
                return;

            UserDefault.AccountPhoneNumber = phoneNumber;

            MoveToEmergencyDisclaimerViewController();
        }

        private bool PhoneNumberValidate(string phoneNumber)
        {
            var isValid = Validation.IsValidPhoneNumber(phoneNumber);

            _phoneValidationLabel.Hidden = isValid;
            return isValid;
        }

        private void OnSkipButtonTouchUpInside(object sender, EventArgs args)
        {
            MoveToEmergencyDisclaimerViewController();
        }

	    private void MoveToEmergencyDisclaimerViewController()
	    {
            var emergencyDisclaimerController = AppDelegate.GetViewController<EmergencyDisclaimerViewController>();
            emergencyDisclaimerController.SelectedAccount = SelectedAccount;
            emergencyDisclaimerController.ParentController = ParentController;

            var navigationController = new UINavigationController(emergencyDisclaimerController);
            Theme.TransitionController(navigationController);
        }

        #region Controls Initialization

        private void InitializeFirstLabel()
        {
            var labelFrame = new CGRect(15, 20, Theme.ScreenBounds.Width - 30, 108);
            _firstLabel = new UILabel(labelFrame)
            {
                Text = $"To make calls with this app, we require your device's phone number and you must enable the Caller ID feature.{Environment.NewLine}This information is not shared with any 3rd parties. Please do not enter your FreedomVoice number here.",
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                TextColor = Theme.BlackColor,
                TextAlignment = UITextAlignment.Left,
                Lines = 6
            };

            View.Add(_firstLabel);
        }

        private void InitializeSecondLabel()
        {
            var labelFrame = new CGRect(15, _firstLabel.Frame.Y + _firstLabel.Frame.Height + 15, Theme.ScreenBounds.Width - 30, 54);
            _secondLabel = new UILabel(labelFrame)
            {
                Text = "You may update this number by going to your device's Settings screen and selecting the FreedomVoice app.",
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                TextColor = Theme.BlackColor,
                TextAlignment = UITextAlignment.Left,
                Lines = 3
            };

            View.Add(_secondLabel);
        }

        private void InitializePhoneNumberTextField()
        {
            const int maxCharacters = 10;

            var textFieldFrame = new CGRect(35, _secondLabel.Frame.Y + _secondLabel.Frame.Height + 25, Theme.ScreenBounds.Width - 120, 44);
            _phoneNumberTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.TextFieldTextColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular),
                BorderStyle = UITextBorderStyle.RoundedRect,
                KeyboardType = UIKeyboardType.NumberPad,
                AttributedPlaceholder = new NSAttributedString("10-Digit Mobile Phone Number", new UIStringAttributes { ForegroundColor = Theme.TextFieldHintColor })
            };
            _phoneNumberTextField.Layer.CornerRadius = 5;
            _phoneNumberTextField.Layer.BorderWidth = 1;
            _phoneNumberTextField.Layer.BorderColor = Theme.TextFieldBorderColor.ToCGColor();
            _phoneNumberTextField.ShouldChangeCharacters = (textField, range, replacement) =>
            {
                var newContent = new NSString(textField.Text).Replace(range, new NSString(replacement)).ToString();
                int number;
                return newContent.Length <= maxCharacters && (replacement.Length == 0 || int.TryParse(replacement, out number));
            };

            View.AddSubview(_phoneNumberTextField);
        }

        private void InitializePlusOneLabel()
        {
            var labelFrame = new CGRect(10, _secondLabel.Frame.Y + _secondLabel.Frame.Height + 25, 30, 44);
            _plusOneLabel = new UILabel(labelFrame)
            {
                Text = "+1",
                TextColor = Theme.BlackColor,
                TextAlignment = UITextAlignment.Left,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular)
            };
            _plusOneLabel.Center = new CGPoint(_plusOneLabel.Center.X, _phoneNumberTextField.Center.Y - 1);

            View.Add(_plusOneLabel);
        }

        private void InitializeContinueButton()
        {
            _continuedButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(_phoneNumberTextField.Frame.Width + 40, _phoneNumberTextField.Frame.Y, 65, 44),
                TintColor = Theme.WhiteColor,
                ClipsToBounds = true
            };
            _continuedButton.Layer.CornerRadius = 5;
            _continuedButton.SetImage(UIImage.FromFile("arrow_right.png"), UIControlState.Normal);
            _continuedButton.TouchUpInside += OnArrowButtonTouchUpInside;

            View.AddSubview(_continuedButton);
        }

        private void InitializePhoneValidationLabel()
        {
            var labelFrame = new CGRect(35, _phoneNumberTextField.Frame.Y + _phoneNumberTextField.Frame.Height + 3, Theme.ScreenBounds.Width - 50, 14);
            _phoneValidationLabel = new UILabel(labelFrame)
            {
                Text = "Please enter a valid 10-digit number.",
                TextColor = Theme.InvalidLabelColor,
                TextAlignment = UITextAlignment.Left,
                Font = UIFont.SystemFontOfSize(12, UIFontWeight.Regular),
                Hidden = true
            };
            View.Add(_phoneValidationLabel);
        }

        #endregion
    }
}