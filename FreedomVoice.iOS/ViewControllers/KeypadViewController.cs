using System;
using System.Drawing;
using CoreGraphics;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using MRoundedButton;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class KeypadViewController : BaseViewController
    {
        protected override string PageName => "Keypad Screen";

        private UILabel _phoneLabel;

        private UIButton _clearPhone;
        private UIButton _keypadDial;

        private string PhoneNumber { get; set; } = string.Empty;

        private CallerIdView CallerIdView { get; set; }

        private readonly IPhoneFormatter phoneFormatter = ServiceContainer.Resolve<IPhoneFormatter>();

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public KeypadViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            CallerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers());

            var keypadLineView = new LineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)Theme.ScreenBounds.Width, 0.5f));

            _phoneLabel = new UILabel
            {
                Frame = new CGRect(40, keypadLineView.Frame.Y, Theme.ScreenBounds.Width - 80, Theme.KeypadPhoneLabelHeight),
                Font = UIFont.SystemFontOfSize(30, UIFontWeight.Thin),
                TextAlignment = UITextAlignment.Center,
                LineBreakMode = UILineBreakMode.HeadTruncation,
                AdjustsFontSizeToFitWidth = true,
                MinimumScaleFactor = 0.8f
            };

            _clearPhone = new UIButton(new CGRect(Theme.ScreenBounds.Width - 43, _phoneLabel.Frame.Y + 5, 40, 40));
            _clearPhone.SetBackgroundImage(UIImage.FromFile("keypad_backspace.png"), UIControlState.Normal);
            _clearPhone.TouchUpInside += OnClearPhoneTouchUpInside;
            _clearPhone.AddGestureRecognizer(new UILongPressGestureRecognizer(ClearPhoneButtonLongPressed));
            ChangeClearPhoneButtonVisibility();

            View.AddSubviews(CallerIdView, keypadLineView, _phoneLabel, _clearPhone);

            var keypadPositionX = (Theme.ScreenBounds.Width - Theme.KeypadWidth) / 2;
            var keypadPositionY = keypadLineView.Frame.Y + _phoneLabel.Frame.Height + Theme.KeypadTopPadding;

            var dialData = new KeypadDial(keypadPositionX, keypadPositionY, Theme.KeypadButtonDiameter, Theme.KeypadDistanceX, Theme.KeypadDistanceY);

            foreach (var item in dialData.Items)
            {
                var buttonRect = new CGRect(item.X, item.Y, Theme.KeypadButtonDiameter, Theme.KeypadButtonDiameter);
                var button = new FVRoundedButton(buttonRect, string.IsNullOrEmpty(item.Image) ? RoundedButtonStyle.Subtitle : RoundedButtonStyle.CentralImage, item.Text)
                {
                    BorderColor = Theme.KeypadBorderColor,
                    TextLabel = { Text = item.Text, Font = UIFont.SystemFontOfSize(Theme.KeypadButtonFontSize, UIFontWeight.Thin) },
                    DetailTextLabel = { Text = item.DetailedText, Font = UIFont.SystemFontOfSize(item.Text == "0" ? 12 : 9, UIFontWeight.Regular) },
                    ContentEdgeInsets = string.IsNullOrEmpty(item.Image) ? new UIEdgeInsets(-4, 0, 0, 0) : UIEdgeInsets.Zero,
                    CornerRadius = RoundedButton.MaxValue,
                    BorderWidth = 1,
                    ContentColor = UIColor.Black
                };


                if (!string.IsNullOrEmpty(item.Image))
                {                    
                    button.ImageView.Image = UIImage.FromFile(item.Image);
                    button.ImageView.ContentMode = UIViewContentMode.Center;
                }

                button.TouchUpInside += OnKeypadButtonTouchUpInside;

                if (item.DetailedText == KeypadDial.Plus)
                    button.AddGestureRecognizer(new UILongPressGestureRecognizer(PlusButtonLongPressed));

                View.AddSubview(button);
            }

            _keypadDial = new UIButton(new CGRect(0, keypadPositionY + Theme.KeypadHeight, Theme.KeypadDialButtonWidth, Theme.KeypadDialButtonHeight));
            _keypadDial.Center = new CGPoint(View.Center.X, _keypadDial.Center.Y);
            _keypadDial.SetBackgroundImage(Theme.KeypadDialImage, UIControlState.Normal);
            _keypadDial.TouchUpInside += OnKeypadDialTouchUpInside;

            View.AddSubview(_keypadDial);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = "Keypad";

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            AppDelegate.EnableUserInteraction(UIApplication.SharedApplication);

            base.ViewDidAppear(animated);
        }

        private void OnKeypadButtonTouchUpInside(object sender, EventArgs args)
        {
            var button = sender as RoundedButton;
            if (button == null) return;

            PhoneNumber += button.TextLabel.Text;
            _phoneLabel.Text = phoneFormatter.Reformat(PhoneNumber);
            ChangeClearPhoneButtonVisibility();
        }

        private void PlusButtonLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Began) return;

            PhoneNumber += "+";
            _phoneLabel.Text = phoneFormatter.Reformat(PhoneNumber);
            ChangeClearPhoneButtonVisibility();
        }

        private void ClearPhoneButtonLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (recognizer.State != UIGestureRecognizerState.Began) return;

            ClearEnteredPhone();
            ChangeClearPhoneButtonVisibility();
        }

        private void OnClearPhoneTouchUpInside(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length <= 1)
            {
                ClearEnteredPhone();
            }
            else
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
                _phoneLabel.Text = phoneFormatter.Reformat(PhoneNumber);
            }
            ChangeClearPhoneButtonVisibility();
        }

        private void ChangeClearPhoneButtonVisibility()
        {
            _clearPhone.Hidden = string.IsNullOrEmpty(PhoneNumber);
        }

        private void ClearEnteredPhone()
        {
            PhoneNumber = string.Empty;
            _phoneLabel.Text = string.Empty;
        }

        private async void OnKeypadDialTouchUpInside(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                if (Recents.RecentsCount == 0) return;

                PhoneNumber = Recents.GetLastRecent().PhoneNumber;
                _phoneLabel.Text = phoneFormatter.Reformat(PhoneNumber);
                ChangeClearPhoneButtonVisibility();
                return;
            }

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, PhoneNumber, this, OnSuccessCallReservation);
        }

        private void OnSuccessCallReservation()
        {
            Recents.AddRecent(PhoneNumber);
            ClearEnteredPhone();
            ChangeClearPhoneButtonVisibility();
        }
    }
}