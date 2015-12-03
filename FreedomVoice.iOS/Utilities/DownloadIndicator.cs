using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public sealed class DownloadIndicator : UIView
    {
        private readonly UIProgressView _progressBar;

        public DownloadIndicator(CGRect frame) : base(frame)
        {
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat centerX = Frame.Width/2;
            nfloat centerY = Frame.Height/2;

            BackgroundColor = UIColor.FromRGBA(0, 0, 0, 190);

            _progressBar = new UIProgressView(UIProgressViewStyle.Bar) { AutoresizingMask = UIViewAutoresizing.All };
            _progressBar.Frame = new CGRect(15, centerY - _progressBar.Frame.Height - 10, Frame.Width - 30, _progressBar.Frame.Height);

            var cancelDownloadButton = new UIButton(UIButtonType.System)
            {
                BackgroundColor = UIColor.FromRGB(244, 244, 244),
                Frame = new CGRect(10, Frame.Height - 67, Frame.Width - 20, 57),
                Font = UIFont.SystemFontOfSize(20, UIFontWeight.Medium),
                Alpha = 1,
                ClipsToBounds = true
            };
            cancelDownloadButton.Layer.CornerRadius = 5;
            cancelDownloadButton.Center = new CGPoint(Center.X, cancelDownloadButton.Center.Y);
            cancelDownloadButton.SetTitle("Cancel", UIControlState.Normal);
            cancelDownloadButton.SetTitleColor(UIColor.FromRGB(0, 122, 255), UIControlState.Normal);
            cancelDownloadButton.TouchUpInside += (sender, args) => AppDelegate.CancelActiveDownload();

            nfloat labelWidth = Frame.Width - 30;
            var loadingLabel = new UILabel(new CGRect(centerX - labelWidth/2, centerY + 20, labelWidth, 22))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "Downloading file...",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };

            AddSubview(_progressBar);
            AddSubview(cancelDownloadButton);
            AddSubview(loadingLabel);
        }

        public void SetDownloadProgress(float percentsComplete)
        {
            _progressBar.Progress = percentsComplete;
        }

        public void Show()
        {
            Alpha = 1;
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