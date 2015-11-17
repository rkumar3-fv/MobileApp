using System;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views.Shared;
using GoogleAnalytics.iOS;
using MRoundedButton;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class KeypadViewController : UIViewController
    {
        private UILabel _phoneLabel;

        private UIButton _clearPhone;
        private UIButton _keypadDial;

        private string PhoneNumber { get; set; } = string.Empty;

        private CallerIdView CallerIdView { get; set; }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public KeypadViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            CallerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers());

            var keypadLineView = new LineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)Theme.ScreenBounds.Width, 0.5f));

            _phoneLabel = new UILabel(new CGRect(30, keypadLineView.Frame.Y, Theme.ScreenBounds.Width - 60, 52))
            {
                Font = UIFont.SystemFontOfSize(30, UIFontWeight.Thin),
                TextAlignment = UITextAlignment.Center
            };

            _clearPhone = new UIButton(new CGRect(Theme.ScreenBounds.Width - 43, _phoneLabel.Frame.Y + 5, 40, 40));
            _clearPhone.SetBackgroundImage(UIImage.FromFile("keypad_backspace.png"), UIControlState.Normal);
            _clearPhone.TouchUpInside += OnClearPhoneTouchUpInside;

            View.AddSubviews(CallerIdView, keypadLineView, _phoneLabel, _clearPhone);

            var keypadPositionX = (Theme.ScreenBounds.Width - Theme.KeypadWidth) / 2;
            var keypadPositionY = keypadLineView.Frame.Y + _phoneLabel.Frame.Height + Theme.KeypadTopPadding;

            var dialData = new KeypadDial(keypadPositionX, keypadPositionY, Theme.KeypadButtonDiameter, Theme.KeypadDistanceX, Theme.KeypadDistanceY);

            foreach (var item in dialData.Items)
            {
                var buttonRect = new CGRect(item.X, item.Y, Theme.KeypadButtonDiameter, Theme.KeypadButtonDiameter);
                var button = new RoundedButton(buttonRect, string.IsNullOrEmpty(item.Image) ? RoundedButtonStyle.Subtitle : RoundedButtonStyle.CentralImage, item.Text)
                {
                    BorderColor = Theme.KeypadBorderColor,
                    TextLabel = { Text = item.Text, Font = UIFont.SystemFontOfSize(36, UIFontWeight.Thin) },
                    DetailTextLabel = { Text = item.DetailedText, Font = UIFont.SystemFontOfSize(9, UIFontWeight.Regular) },
                    CornerRadius = RoundedButton.MaxValue,
                    BorderWidth = 1,
                    ContentColor = UIColor.Black,
                    ContentAnimateToColor = Theme.KeypadBorderColor
                };

                if (!string.IsNullOrEmpty(item.Image))
                {                    
                    button.ImageView.Image = UIImage.FromFile(item.Image);
                    button.ImageView.ContentMode = UIViewContentMode.Center;                    
                }                   

                button.TouchUpInside += OnKeypadButtonTouchUpInside(item);

                if (item.DetailedText == KeypadDial.Plus)
                {
                    var gestureRecognizer = new UILongPressGestureRecognizer(this, new ObjCRuntime.Selector("HandleLongPress:"));
                    button.AddGestureRecognizer(gestureRecognizer);
                }

                View.AddSubview(button);
            }

            _keypadDial = new UIButton(new CGRect(0, keypadPositionY + Theme.KeypadHeight, Theme.KeypadDialButtonDiameter, Theme.KeypadDialButtonDiameter));
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
            base.ViewDidAppear(animated);

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Keypad Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

        private EventHandler OnKeypadButtonTouchUpInside(DialItem item)
        {
            return (sender, ea) =>
            {
                PhoneNumber += item.Text;
                _phoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            };
        }

        [Export("HandleLongPress:")]
        public void ButtonLongPressed(UILongPressGestureRecognizer recognizer)
        {
            if (!string.IsNullOrEmpty(PhoneNumber)) return;

            PhoneNumber += KeypadDial.Plus;
            _phoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
        }

        private void OnClearPhoneTouchUpInside(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length <= 1)
            {
                PhoneNumber = string.Empty;
                _phoneLabel.Text = string.Empty;
            }
            else
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
                _phoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }

        private void OnKeypadDialTouchUpInside(object sender, EventArgs args)
        {
            string phoneNumberForCallReservation;

            if (string.IsNullOrEmpty(PhoneNumber))
            {
                if (MainTabBarInstance.Recents.Count != 0)
                    phoneNumberForCallReservation = MainTabBarInstance.Recents.Last().PhoneNumber;
                else
                    return;
            }
            else
                phoneNumberForCallReservation = PhoneNumber;

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phoneNumberForCallReservation, NavigationController);

            AddRecent(phoneNumberForCallReservation);
        }
        
	    private static void AddRecent(string phoneNumber)
        {
            MainTabBarInstance.Recents.Add(new Recent(string.Empty, phoneNumber, DateTime.Now));
        }
    }
}