using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public sealed class LoadingIndicator : UIView
    {
        public readonly UIActivityIndicatorView ActivityIndicator;
        public readonly UIProgressView ProgressBar;
        public readonly UIButton CancelDownloadButton;

        public LoadingIndicator(CGRect frame, ProgressControlType loadingControl, CGPoint indicatorCenter) : base(frame)
        {
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            if (loadingControl == ProgressControlType.ActivityIndicator)
            {
                BackgroundColor = UIColor.Clear;
                ActivityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
                {
                    HidesWhenStopped = true,
                    Frame = new CGRect(0, 0, 37, 37),
                    AutoresizingMask = UIViewAutoresizing.All,
                    Center = indicatorCenter != CGPoint.Empty ? indicatorCenter : new CGPoint(centerX, centerY)
                };

                AddSubview(ActivityIndicator);
            }
            else if (loadingControl == ProgressControlType.ProgressBar)
            {
                BackgroundColor = UIColor.FromRGBA(0, 0, 0, 190);

                ProgressBar = new UIProgressView(UIProgressViewStyle.Bar);
                ProgressBar.Frame = new CGRect(15, centerY - ProgressBar.Frame.Height - 10, Frame.Width - 30, ProgressBar.Frame.Height);
                ProgressBar.AutoresizingMask = UIViewAutoresizing.All;

                AddSubview(ProgressBar);

                CancelDownloadButton = new UIButton(UIButtonType.System)
                {
                    BackgroundColor = UIColor.FromRGB(244, 244, 244),
                    Alpha = 1,
                    Frame = new CGRect(10, Frame.Height - 67, Frame.Width - 20, 57),
                    Font = UIFont.SystemFontOfSize(20, UIFontWeight.Medium),
                    ClipsToBounds = true
                };
                CancelDownloadButton.Layer.CornerRadius = 5;
                CancelDownloadButton.Center = new CGPoint(Center.X, CancelDownloadButton.Center.Y);
                CancelDownloadButton.SetTitle("Cancel", UIControlState.Normal);
                CancelDownloadButton.SetTitleColor(UIColor.FromRGB(0, 122, 255), UIControlState.Normal);

                AddSubview(CancelDownloadButton);

                nfloat labelWidth = Frame.Width - 30;
                var loadingLabel = new UILabel(new CGRect(centerX - labelWidth / 2, centerY + 20, labelWidth, 22))
                {
                    BackgroundColor = UIColor.Clear,
                    TextColor = UIColor.White,
                    Text = "Downloading file...",
                    TextAlignment = UITextAlignment.Center,
                    AutoresizingMask = UIViewAutoresizing.All
                };

                AddSubview(loadingLabel);
            }
        }

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            Animate(0.5, () => { Alpha = 0; }, RemoveFromSuperview);
        }
    }

    public enum ProgressControlType
    {
        ProgressBar,
        ActivityIndicator
    }
}