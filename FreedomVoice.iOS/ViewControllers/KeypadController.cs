using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Helpers;
using MRoundedButton;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UIKit;

namespace FreedomVoice.iOS
{
	partial class KeypadController : UIViewController
	{

		public KeypadController (IntPtr handle) : base (handle)
		{
            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            foreach (DialItem item in DialData.GetDialItems()) {

                CGRect buttonRect = new CGRect(item.X, item.Y, item.Width, item.Height);
                RoundedButton button = new RoundedButton(buttonRect,
                       RoundedButtonStyle.Subtitle,
                       item.Text
                   );
                
                button.TouchUpInside += (sender, ea) => {
                    PhoneLabel.Text += item.Text;                    
                };

                button.BorderColor = UIColor.Clear;                
                button.TextLabel.Text = item.Text;
                button.TextLabel.Font = UIFont.BoldSystemFontOfSize(36);
                button.DetailTextLabel.Text = item.DetailedText;
                button.DetailTextLabel.Font = UIFont.SystemFontOfSize(10);
                button.CornerRadius = 35;
                button.BorderWidth = 1;
                button.BorderColor = UIColor.FromRGB(173, 184, 197);
                button.ContentColor = UIColor.Black;
                button.ContentAnimateToColor = UIColor.FromRGB(173, 184, 197);

                View.AddSubview(button);
            }
        }

        partial void ClearPhone_TouchUpInside(UIButton sender)
        {
            if (String.IsNullOrEmpty(PhoneLabel.Text) || PhoneLabel.Text.Length <= 1)
                PhoneLabel.Text = string.Empty;
            else
                PhoneLabel.Text = PhoneLabel.Text.Substring(0, PhoneLabel.Text.Length - 1);
        }

        partial void KeypadDial_TouchUpInside(UIButton sender)
        {
            //throw new NotImplementedException();
        }
    }
}
