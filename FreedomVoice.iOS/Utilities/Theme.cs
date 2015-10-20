using System;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class Theme
    {
        #region Images

        

        #endregion

        #region Colors

        static readonly Lazy<UIColor> indicatorColor = new Lazy<UIColor>(() => DarkGrayColor);

        /// <summary>
        /// General indicator color for the entire app
        /// </summary>
        public static UIColor IndicatorColor => indicatorColor.Value;

        static readonly Lazy<UIColor> backgroundColor = new Lazy<UIColor>(() => UIColor.FromRGB(0xef, 0xef, 0xef));

        /// <summary>
        /// General background color for the app
        /// </summary>
        public static UIColor BackgroundColor => backgroundColor.Value;

        static readonly Lazy<UIColor> darkGrayColor = new Lazy<UIColor>(() => UIColor.FromRGB(0x73, 0x81, 0x82));

        /// <summary>
        /// Dark gray color used on iOS 7
        /// </summary>
        public static UIColor DarkGrayColor => darkGrayColor.Value;

        #endregion

        /// <summary>
        /// Apply UIAppearance to this application, this is iOS's version of "styling"
        /// </summary>
        public static void Apply()
        {
            UIActivityIndicatorView.Appearance.Color = IndicatorColor;

            //UIToolbar.Appearance.SetBackgroundImage(BlueBar, UIToolbarPosition.Any, UIBarMetrics.Default);

            UINavigationBar.Appearance.TintColor = UIColor.White;
            UINavigationBar.Appearance.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            UINavigationBar.Appearance.BarTintColor = new UIColor(0.016f, 0.588f, 0.816f, 1);
        }

        const string FontName = "HelveticaNeue-Light";
        const string BoldFontName = "HelveticaNeue-Medium";

        /// <summary>
        /// Returns the default font with a certain size
        /// </summary>
        public static UIFont FontOfSize(float size)
        {
            return UIFont.FromName(FontName, size);
        }

        /// <summary>
        /// Returns the default font with a certain size
        /// </summary>
        public static UIFont BoldFontOfSize(float size)
        {
            return UIFont.FromName(BoldFontName, size);
        }

        /// <summary>
        /// Transitions a controller to the rootViewController, for a fullscreen transition
        /// </summary>
        public static void TransitionController(UIViewController controller, bool animated = true)
        {
            var window = ServiceContainer.Resolve<UIWindow>();

            //Return if it's already the root controller
            if (window.RootViewController == controller)
                return;

            //Set the root controller
            window.RootViewController = controller;

            //Peform an animation, note that null is not allowed as a callback, so I use delegate { }
            if (animated)
                UIView.Transition(window, .3, UIViewAnimationOptions.TransitionCrossDissolve, delegate { }, delegate { });
        }
    }
}