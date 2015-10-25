using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using GoogleAnalytics.iOS;
using MRoundedButton;
using System;
using System.Collections.Generic;
using System.Drawing;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class KeypadViewController : UIViewController
    {
		public KeypadViewController (IntPtr handle) : base (handle) { }
        UILabel PhoneLabel;
        UIButton ClearPhone, KeypadDial;

        public CallerIdView CallerIdView { get; private set; }

        private string PhoneNumber { get; set; } = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PhoneLabel = new UILabel(new CGRect(12, 110, 250, 32)) {
                Font = UIFont.SystemFontOfSize(28, UIFontWeight.Thin),
                TextAlignment = UITextAlignment.Right
            };

            ClearPhone = new UIButton(new CGRect(260, 110, 40, 40));
            ClearPhone.SetBackgroundImage(UIImage.FromFile("backspace.png"), UIControlState.Normal);
            ClearPhone.TouchUpInside += ClearPhone_TouchUpInside;

            KeypadDial = new UIButton(new CGRect(130, 450, 62, 62));
            KeypadDial.SetBackgroundImage(UIImage.FromFile("dial.png"), UIControlState.Normal);
            KeypadDial.TouchUpInside += KeypadDial_TouchUpInside;

            View.AddSubviews(PhoneLabel, ClearPhone, KeypadDial);

            foreach (var item in DialData.Items)
            {
                var buttonRect = new CGRect(item.X, item.Y, item.Width, item.Height);
                var button = new RoundedButton(buttonRect, RoundedButtonStyle.Subtitle, item.Text)
                {
                    BorderColor = UIColor.FromRGB(173, 184, 197),
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
                    ContentAnimateToColor = UIColor.FromRGB(173, 184, 197)
                };

                button.TouchUpInside += (sender, ea) => {
                    PhoneNumber += item.Text;
                    PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
                };

                View.AddSubview(button);
            }

            CallerIdView = new CallerIdView(new RectangleF(0, 65, 320, 44), MainTab.GetPresentationNumbers());
            View.AddSubviews(CallerIdView);
        }

        void ClearPhone_TouchUpInside(object sender, EventArgs args)
        {
            if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length <= 1)
            {
                PhoneNumber = string.Empty;
                PhoneLabel.Text = string.Empty;
            }
            else
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
                PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }

        void KeypadDial_TouchUpInside(object sender, EventArgs args)
        {
            AddRecent();            
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Keypad Screen");
            //GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

        private MainTabBarController MainTab { get { return ParentViewController.ParentViewController as MainTabBarController; } }

        private void AddRecent()
        {            
            MainTab?.Recents.Add(new Recent(string.Empty, PhoneNumber, DateTime.Now));
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            PresentationNumber selectedNumber = MainTab?.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);
        }
    }
}