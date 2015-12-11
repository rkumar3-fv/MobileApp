using System;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class SplashViewController : BaseViewController
	{
	    protected override string PageName => "Splash Screen";

	    public SplashViewController(IntPtr handle) : base(handle) { }

        public override void ViewWillAppear(bool animated)
        {
            View.BackgroundColor = UIColor.FromPatternImage(Theme.SplashScreenImage);
        }
    }
}