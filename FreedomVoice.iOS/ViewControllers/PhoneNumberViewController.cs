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

        private UILabel _infoLabel;
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

            InitializeInfoLabel();
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

        private void InitializeInfoLabel()
        {
            var labelFrame = new CGRect(15, 20, Theme.ScreenBounds.Width - 30, Theme.PhoneNumberInformationLabelHeight);
            _infoLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                TextColor = Theme.BlackColor,
                TextAlignment = UITextAlignment.Left,
                Lines = Theme.PhoneNumberInformationLabelLines
            };

            if (Theme.ScreenBounds.Height > 480)
            {
                _infoLabel.Text = $"To make calls with this app, we require your device's phone number and you must enable the Caller ID feature.{Environment.NewLine}" +
                                  $"This information is not shared with any 3rd parties. Please do not enter your FreedomVoice number here.{Environment.NewLine}{Environment.NewLine}" +
                                  "You may update this number by going to your device's Settings screen and selecting the FreedomVoice app.";
            }
            else
            {
                const string infoText = "To make outgoing calls, enter your mobile phone number here and turn on Caller ID for your phone. We will not share your number with any 3rd parties.";
                var textAttributedString = new NSMutableAttributedString(infoText);
                textAttributedString.AddAttribute(UIStringAttributeKey.Font, UIFont.SystemFontOfSize(15, UIFontWeight.Semibold), new NSRange(35, 6));

                _infoLabel.AttributedText = textAttributedString;
            }

            View.Add(_infoLabel);
        }

        private void InitializePhoneNumberTextField()
        {
            const int maxCharacters = 10;

            var placeholderText = Theme.ScreenBounds.Width > 320 ? "10-Digit Mobile Phone Number" : "10-Digit Mobile Number";

            var textFieldFrame = new CGRect(35, _infoLabel.Frame.Y + _infoLabel.Frame.Height + 22, Theme.ScreenBounds.Width - 120, 44);
            _phoneNumberTextField = new UITextField(textFieldFrame)
            {
                TextColor = Theme.TextFieldTextColor,
                BackgroundColor = UIColor.Clear,
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Regular),
                BorderStyle = UITextBorderStyle.RoundedRect,
                KeyboardType = UIKeyboardType.NumberPad,
                AttributedPlaceholder = new NSAttributedString(placeholderText, new UIStringAttributes { ForegroundColor = Theme.TextFieldHintColor })
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
            var labelFrame = new CGRect(10, _phoneNumberTextField.Frame.Y, 30, 44);
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