using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.SharedViews;
using GoogleAnalytics.iOS;
using MRoundedButton;
using System;
using System.Collections.Generic;
using System.Drawing;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class KeypadViewController : UIViewController
    {
		public KeypadViewController (IntPtr handle) : base (handle) { }

        private string PhoneNumber { get; set; } = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            PhoneLabel.Font = UIFont.SystemFontOfSize(28, UIFontWeight.Thin);

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

            var callerIdView = new CallerIdView(new RectangleF(0, 65, 320, 44), new List<PresentationNumber> { new PresentationNumber("1112223333"), new PresentationNumber("4445556666"), new PresentationNumber("7778889999") });
            View.AddSubviews(callerIdView);
        }

        partial void ClearPhone_TouchUpInside(UIButton sender)
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

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            //GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Keypad Screen");
            //GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }
    }
}
