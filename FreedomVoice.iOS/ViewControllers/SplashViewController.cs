using System;
using CoreGraphics;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class SplashViewController : UIViewController
	{
	    public SplashViewController(IntPtr handle) : base(handle) { }

        private UIActivityIndicatorView _activityIndicator;

        public override void ViewWillAppear(bool animated)
	    {
	        NavigationController.NavigationBar.Hidden = true;
            View.BackgroundColor = UIColor.FromPatternImage(Theme.SplashScreenImage);

            InitializeActivityIndicator();
	    }

	    public override void ViewDidDisappear(bool animated)
	    {
            _activityIndicator.StopAnimating();

            base.ViewDidDisappear(animated);
        }

	    private void InitializeActivityIndicator()
        {
            var frame = new CGRect(0, 0, 37, 37);
            _activityIndicator = new UIActivityIndicatorView(frame)
            {
                ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge,
                HidesWhenStopped = true,
                Center = View.Center
            };
            _activityIndicator.StartAnimating();

            View.AddSubview(_activityIndicator);
        }
    }
}