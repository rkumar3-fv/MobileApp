using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.SharedViews;
using MRoundedButton;
using System;
using System.Drawing;
using FreedomVoice.Core.Utils;
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

            foreach (var item in DialData.Items)
            {
                var buttonRect = new CGRect(item.X, item.Y, item.Width, item.Height);
                var button = new RoundedButton(buttonRect, RoundedButtonStyle.Subtitle, item.Text)
                {
                    BorderColor = UIColor.Clear,
                    TextLabel =
                    {
                        Text = item.Text,
                        Font = UIFont.SystemFontOfSize(32)
                    },
                    DetailTextLabel =
                    {
                        Text = item.DetailedText,
                        Font = UIFont.SystemFontOfSize(10)
                    },
                    CornerRadius = 30,
                    BorderWidth = 1,
                    ContentColor = UIColor.Black,
                    ContentAnimateToColor = UIColor.FromRGB(173, 184, 197)
                };

                button.BorderColor = UIColor.FromRGB(173, 184, 197);

                button.TouchUpInside += (sender, ea) => {
                    PhoneNumber += item.Text;
                    PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
                };

                View.AddSubview(button);
            }

            var callerIdView = new CallerIdView(new RectangleF(0, 65, 320, 44));
            View.AddSubviews(callerIdView);
        }

        partial void ClearPhone_TouchUpInside(UIButton sender)
        {
            if (string.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length <= 1)
                PhoneLabel.Text = string.Empty;
            else
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
                PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }
    }
}