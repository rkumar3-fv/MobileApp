using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class AccountUnavailableViewController : UIViewController
	{
        public Account SelectedAccount { private get; set; }
	    public UINavigationController ParentController { private get; set; }

        private UIImageView _logoImage;
        private UILabel _accountNotAvailableLabel;
        private UILabel _callCustomerCareLabel;
        private UIButton _callCustomerCareButton;

        public AccountUnavailableViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            Title = SelectedAccount.FormattedPhoneNumber;

            InitializeEmergencyDisclaimerImage();
            InitializeAccountNotAvailableLabel();
            InitializeCallCustomerCareLabel();
            InitializeCallCustomerCareButton();

            if (ParentController?.TopViewController is AccountsViewController)
                NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => Theme.TransitionController(ParentController), "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), true);

            base.ViewDidLoad();
        }

        private static void OnCallCustomerCareTouchUpInside(object sender, EventArgs args)
	    {
            var url = new NSUrl("tel:+18004771477");

            if (!UIApplication.SharedApplication.OpenUrl(url))
                new UIAlertView("Not supported", "Your device does not appear to support making cellular voice calls.", null, "OK", null).Show();
        }

        #region Controls Initialization

        private void InitializeEmergencyDisclaimerImage()
        {
            _logoImage = new UIImageView(UIImage.FromFile("logo_freedomvoice_grey.png"));
            _logoImage.Frame = new CGRect(0, 122, _logoImage.Image.CGImage.Width / 2, _logoImage.Image.CGImage.Height / 2);
            _logoImage.Center = new CGPoint(View.Center.X, _logoImage.Center.Y);

            View.AddSubview(_logoImage);
        }

        private void InitializeAccountNotAvailableLabel()
        {
            var labelFrame = new CGRect(0, _logoImage.Frame.Y + _logoImage.Frame.Height + 27, Theme.ScreenBounds.Width, 40);
            _accountNotAvailableLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                Text = $"The account you have selected{Environment.NewLine}is not available.",
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 2
            };
            _accountNotAvailableLabel.Center = new CGPoint(View.Center.X, _accountNotAvailableLabel.Center.Y);

            View.Add(_accountNotAvailableLabel);
        }

        private void InitializeCallCustomerCareLabel()
        {
            var labelFrame = new CGRect(0, _accountNotAvailableLabel.Frame.Y + _accountNotAvailableLabel.Frame.Height + 20, Theme.ScreenBounds.Width, 40);
            _callCustomerCareLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                Text = $"Please contact Customer Care{Environment.NewLine}at (800) 477-1477.",
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 2
            };
            _callCustomerCareLabel.Center = new CGPoint(View.Center.X, _callCustomerCareLabel.Center.Y);

            View.Add(_callCustomerCareLabel);
        }

        private void InitializeCallCustomerCareButton()
        {
            _callCustomerCareButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _callCustomerCareLabel.Frame.Y + _callCustomerCareLabel.Frame.Height + 40, Theme.ScreenBounds.Width - 30, 44),
                Font = UIFont.SystemFontOfSize(21, UIFontWeight.Medium),
                ClipsToBounds = true
            };
            _callCustomerCareButton.Layer.CornerRadius = 5;
            _callCustomerCareButton.Center = new CGPoint(View.Center.X, _callCustomerCareButton.Center.Y);
            _callCustomerCareButton.SetTitle("Call Customer Care", UIControlState.Normal);
            _callCustomerCareButton.SetTitleColor(Theme.WhiteColor, UIControlState.Normal);
            _callCustomerCareButton.TouchUpInside += OnCallCustomerCareTouchUpInside;

            View.AddSubview(_callCustomerCareButton);
        }

        #endregion
    }
}