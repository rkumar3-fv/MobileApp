using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using GoogleAnalytics.iOS;
using MRoundedButton;
using System;
using System.Drawing;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Views.Shared;
using UIKit;
using Foundation;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class KeypadViewController : UIViewController
    {
		public KeypadViewController (IntPtr handle) : base (handle) { }
        UILabel _phoneLabel;
        UIButton _clearPhone, _keypadDial;

        public CallerIdView CallerIdView { get; private set; }

        private string PhoneNumber { get; set; } = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            _phoneLabel = new UILabel(new CGRect(30, 105, 250, 52))
            {
                Font = UIFont.SystemFontOfSize(30, UIFontWeight.Thin),
                TextAlignment = UITextAlignment.Center
            };

            _clearPhone = new UIButton(new CGRect(277, 110, 40, 40));
            _clearPhone.SetBackgroundImage(UIImage.FromFile("keypad_backspace.png"), UIControlState.Normal);
            _clearPhone.TouchUpInside += ClearPhone_TouchUpInside;

            _keypadDial = new UIButton(new CGRect(130, 440, 62, 62));
            _keypadDial.SetBackgroundImage(UIImage.FromFile("keypad_call.png"), UIControlState.Normal);
            _keypadDial.TouchUpInside += KeypadDial_TouchUpInside;

            View.AddSubviews(_phoneLabel, _clearPhone, _keypadDial);

            foreach (var item in DialData.Items)
            {
                var buttonRect = new CGRect(item.X, item.Y, item.Width, item.Height);
                var button = new RoundedButton(buttonRect, string.IsNullOrEmpty(item.Image) ? RoundedButtonStyle.Subtitle : RoundedButtonStyle.CentralImage, item.Text)
                {
                    BorderColor = Theme.KeypadBorderColor,
                    TextLabel =
                    {
                        Text = item.Text,
                        Font = UIFont.SystemFontOfSize(32, UIFontWeight.Thin)
                    },
                    DetailTextLabel =
                    {
                        Text = item.DetailedText,
                        Font = UIFont.SystemFontOfSize(10, UIFontWeight.Thin)
                    },
                    CornerRadius = 30,
                    BorderWidth = 1,
                    ContentColor = UIColor.Black,
                    ContentAnimateToColor = Theme.KeypadBorderColor
                };

                if (!string.IsNullOrEmpty(item.Image))
                {                    
                    button.ImageView.Image = UIImage.FromFile(item.Image);
                    button.ImageView.ContentMode = UIViewContentMode.Center;                    
                }                   

                button.TouchUpInside += (sender, ea) => {
                    PhoneNumber += item.Text;
                    _phoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
                };

                if (item.DetailedText == DialData.PLUS)
                {
                    var gr = new UILongPressGestureRecognizer(this, new ObjCRuntime.Selector("HandleLongPress:"));
                    button.AddGestureRecognizer(gr);
                }

                View.AddSubview(button);
            }

            CallerIdView = new CallerIdView(new RectangleF(0, 65, 320, 40), MainTab.GetPresentationNumbers());
            View.AddSubviews(CallerIdView);
        }

        [Export("HandleLongPress:")]
        public void ButtonLongPressed(UILongPressGestureRecognizer recognizer)
        {            
            if (string.IsNullOrEmpty(PhoneNumber))
            {
                PhoneNumber += DialData.PLUS;
                _phoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }

        void ClearPhone_TouchUpInside(object sender, EventArgs args)
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

        void KeypadDial_TouchUpInside(object sender, EventArgs args)
        {
            AddRecent();            
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Keypad Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

        private MainTabBarController MainTab => ParentViewController.ParentViewController as MainTabBarController;

	    private void AddRecent()
        {            
            MainTab?.Recents.Add(new Recent(string.Empty, PhoneNumber, DateTime.Now));
        }

        public override void ViewWillAppear(bool animated)
        {
            MainTab.Title = "Keypad";

            PresentationNumber selectedNumber = MainTab?.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }
    }
}