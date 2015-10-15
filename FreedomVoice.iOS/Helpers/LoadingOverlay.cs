using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public sealed class LoadingOverlay : UIView
    {
        public LoadingOverlay(CGRect frame) : base(frame)
        {
            BackgroundColor = UIColor.Black;
            Alpha = 0.75f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 20;

            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            var activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(centerX - (activitySpinner.Frame.Width / 2), centerY - activitySpinner.Frame.Height - 20, activitySpinner.Frame.Width, activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            var loadingLabel = new UILabel(new CGRect(centerX - (labelWidth/2), centerY + 20, labelWidth, labelHeight))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "Loading...",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };
            AddSubview(loadingLabel);
        }

        public void Hide()
        {
            Animate(0.5, () => { Alpha = 0; }, RemoveFromSuperview);
        }
    }
}