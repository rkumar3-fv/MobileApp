using System;
using CoreGraphics;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public sealed class LoadingOverlay : UIView
    {
        public readonly UIActivityIndicatorView ActivityIndicator;
        public readonly UIProgressView ProgressBar;
        public readonly UIButton CancelDownloadButton;

        public LoadingOverlay(CGRect frame, BaseViewModel.ProgressControlType loadingControl, string loadingText) : base(frame)
        {
            BackgroundColor = UIColor.FromRGBA(0, 0, 0, 190);
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat labelHeight = 22;
            nfloat labelWidth = Frame.Width - 30;

            // derive the center x and y
            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            if (loadingControl == BaseViewModel.ProgressControlType.ActivityIndicator)
            {
                ActivityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
                ActivityIndicator.Frame = new CGRect(centerX - (ActivityIndicator.Frame.Width / 2), centerY - ActivityIndicator.Frame.Height, ActivityIndicator.Frame.Width, ActivityIndicator.Frame.Height);
                ActivityIndicator.AutoresizingMask = UIViewAutoresizing.All;

                AddSubview(ActivityIndicator);

                ActivityIndicator.StartAnimating();
            }
            else if (loadingControl == BaseViewModel.ProgressControlType.ProgressBar)
            {
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
            }

            // create and configure the "Loading Data" label
            var loadingLabel = new UILabel(new CGRect(centerX - labelWidth / 2, centerY + 20, labelWidth, labelHeight))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = loadingText,
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };

            AddSubview(loadingLabel);
        }

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            Animate(0.5, () => { Alpha = 0; }, RemoveFromSuperview);
        }
    }
}