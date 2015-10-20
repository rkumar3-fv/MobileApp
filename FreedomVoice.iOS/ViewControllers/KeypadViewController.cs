using CoreGraphics;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Helpers;
using MRoundedButton;
using System;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class KeypadViewController : UIViewController
    {
		public KeypadViewController (IntPtr handle) : base (handle)
		{
		}

        private string PhoneNumber { get; set; } = string.Empty;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            foreach (DialItem item in DialData.GetDialItems())
            {

                CGRect buttonRect = new CGRect(item.X, item.Y, item.Width, item.Height);
                RoundedButton button = new RoundedButton(buttonRect,
                       RoundedButtonStyle.Subtitle,
                       item.Text
                   );

                button.TouchUpInside += (sender, ea) => {
                    PhoneNumber += item.Text;
                    PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
                };

                button.BorderColor = UIColor.Clear;
                button.TextLabel.Text = item.Text;
                button.TextLabel.Font = UIFont.SystemFontOfSize(32);
                button.DetailTextLabel.Text = item.DetailedText;
                button.DetailTextLabel.Font = UIFont.SystemFontOfSize(10);
                button.CornerRadius = 30;
                button.BorderWidth = 1;
                button.BorderColor = UIColor.FromRGB(173, 184, 197);
                button.ContentColor = UIColor.Black;
                button.ContentAnimateToColor = UIColor.FromRGB(173, 184, 197);

                View.AddSubview(button);
            }
        }

        partial void ClearPhone_TouchUpInside(UIButton sender)
        {
            if (String.IsNullOrEmpty(PhoneNumber) || PhoneNumber.Length <= 1)
                PhoneLabel.Text = string.Empty;
            else
            {
                PhoneNumber = PhoneNumber.Substring(0, PhoneNumber.Length - 1);
                PhoneLabel.Text = DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }

        partial void KeypadDial_TouchUpInside(UIButton sender)
        {
            //throw new NotImplementedException();
        }
        
    }
}
