using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public sealed class ActivityIndicator : UIView
    {
        private readonly UIActivityIndicatorView _activityIndicator;

        public ActivityIndicator(CGRect frame) : base(frame)
        {
            AutoresizingMask = UIViewAutoresizing.All;

            BackgroundColor = UIColor.Clear;
            _activityIndicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                HidesWhenStopped = true,
                Frame = new CGRect(0, 0, 37, 37),
                AutoresizingMask = UIViewAutoresizing.All,
                Center = Theme.ScreenCenter
            };

            AddSubview(_activityIndicator);
        }

        public void SetActivityIndicatorCenter(CGPoint indicatorCenter)
        {
            _activityIndicator.Center = indicatorCenter;
        }

        public void Show()
        {
            _activityIndicator.Hidden = false;
            _activityIndicator.StartAnimating();
        }

        /// <summary>
        /// Fades out the control and then removes it from the super view
        /// </summary>
        public void Hide()
        {
            _activityIndicator.StopAnimating();
            RemoveFromSuperview();
        }
    }
}