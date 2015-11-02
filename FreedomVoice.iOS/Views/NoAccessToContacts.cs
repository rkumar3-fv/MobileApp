using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    public class NoAccessToContacts : UIView
    {
        private UIImageView _noAccessToContactsImage;
        private UILabel _mainLabel;
        private UILabel _instructionsLabel;
        private UIButton _settingsButton;

        public NoAccessToContacts(CGRect rect) : base(rect)
        {
            BackgroundColor = UIColor.White;
        }

        public override void Draw(CGRect rect)
        {
            InitializeNoAccessToContactsImage();
            InitializeMainLabel();
            InitializeInstructionsLabel();
            InitializeUnderstandButton();

            base.Draw(rect);
        }

        private static void OnSettingsButtonTouchUpInside(object sender, EventArgs e)
        {
            UIApplication.SharedApplication.OpenUrl(new NSUrl(UIApplication.OpenSettingsUrlString));
        }

        #region Controls Initialization

        private void InitializeNoAccessToContactsImage()
        {
            _noAccessToContactsImage = new UIImageView(UIImage.FromFile("contacts_access.png"));
            _noAccessToContactsImage.Frame = new CGRect(0, UIApplication.SharedApplication.StatusBarFrame.Height + 118, _noAccessToContactsImage.Image.CGImage.Width / 2, _noAccessToContactsImage.Image.CGImage.Height / 2);
            _noAccessToContactsImage.Center = new CGPoint(Center.X, _noAccessToContactsImage.Center.Y);

            AddSubview(_noAccessToContactsImage);
        }

        private void InitializeMainLabel()
        {
            var labelFrame = new CGRect(0, _noAccessToContactsImage.Frame.Y + _noAccessToContactsImage.Frame.Height + 15, Theme.ScreenBounds.Width, 42);
            _mainLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium),
                Text = $"FreedomVoice does not have{Environment.NewLine}access to your contacts",
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 2
            };
            _mainLabel.Center = new CGPoint(Center.X, _mainLabel.Center.Y);

            Add(_mainLabel);
        }

        private void InitializeInstructionsLabel()
        {
            var labelFrame = new CGRect(0, _mainLabel.Frame.Y + _mainLabel.Frame.Height + 18, Theme.ScreenBounds.Width, 54);
            _instructionsLabel = new UILabel(labelFrame)
            {
                Font = UIFont.SystemFontOfSize(15, UIFontWeight.Regular),
                Text = $"To enable access go to Settings \u203A{Environment.NewLine}Privacy \u203A Contacts \u203A FreedomVoice{Environment.NewLine}and set the switch to \"On\".",
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Lines = 3
            };
            _instructionsLabel.Center = new CGPoint(Center.X, _instructionsLabel.Center.Y);

            Add(_instructionsLabel);
        }

        private void InitializeUnderstandButton()
        {
            _settingsButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = Theme.ButtonColor,
                Frame = new CGRect(15, _instructionsLabel.Frame.Y + _instructionsLabel.Frame.Height + 40, Theme.ScreenBounds.Width - 30, 44),
                Font = UIFont.SystemFontOfSize(21, UIFontWeight.Medium),
                ClipsToBounds = true
            };
            _settingsButton.Layer.CornerRadius = 5;
            _settingsButton.Center = new CGPoint(Center.X, _settingsButton.Center.Y);
            _settingsButton.SetTitle("Settings", UIControlState.Normal);
            _settingsButton.SetTitleColor(Theme.WhiteColor, UIControlState.Normal);
            _settingsButton.TouchUpInside += OnSettingsButtonTouchUpInside;

            AddSubview(_settingsButton);
        }

        #endregion
    }
}
