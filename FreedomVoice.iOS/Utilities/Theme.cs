using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class Theme
    {
        #region Images



        #endregion

        #region Colors

        /// <summary>
        /// General indicator color for the entire app
        /// </summary>
        static readonly Lazy<UIColor> indicatorColor = new Lazy<UIColor>(() => DarkGrayColor);
        public static UIColor IndicatorColor => indicatorColor.Value;

        /// <summary>
        /// Orange button color
        /// </summary>
        static readonly Lazy<UIColor> buttonColor = new Lazy<UIColor>(() => UIColor.FromRGB(270, 170, 30));
        public static UIColor ButtonColor => buttonColor.Value;

        /// <summary>
        /// Orange button color
        /// </summary>
        static readonly Lazy<UIColor> forgotPasswordButtonColor = new Lazy<UIColor>(() => UIColor.FromRGB(3, 103, 143));
        public static UIColor ForgotPasswordButtonColor => forgotPasswordButtonColor.Value;

        /// <summary>
        /// Login Page text fields border color
        /// </summary>
        static readonly Lazy<UIColor> loginPageTextFieldBorderColor = new Lazy<UIColor>(() => UIColor.FromRGBA(255, 255, 255, 90));
        public static UIColor LoginPageTextFieldBorderColor => loginPageTextFieldBorderColor.Value;

        /// <summary>
        /// Login Page text fields hint text color
        /// </summary>
        static readonly Lazy<UIColor> loginPageTextFieldHintColor = new Lazy<UIColor>(() => UIColor.FromRGBA(255, 255, 255, 128));
        public static UIColor LoginPageTextFieldHintColor => loginPageTextFieldHintColor.Value;

        /// <summary>
        /// Login Page text fields text color
        /// </summary>
        static readonly Lazy<UIColor> loginPageTextFieldTextColor = new Lazy<UIColor>(() => UIColor.White);
        public static UIColor LoginPageTextFieldTextColor => loginPageTextFieldTextColor.Value;

        /// <summary>
        /// Invalid text field border color
        /// </summary>
        static readonly Lazy<UIColor> invalidTextFieldBorderColor = new Lazy<UIColor>(() => DimOrangeColor);
        public static UIColor InvalidTextFieldBorderColor => invalidTextFieldBorderColor.Value;

        /// <summary>
        /// Label color
        /// </summary>
        static readonly Lazy<UIColor> labelColor = new Lazy<UIColor>(() => UIColor.Black);
        public static UIColor LabelColor => labelColor.Value;

        /// <summary>
        /// Invalid label color
        /// </summary>
        static readonly Lazy<UIColor> invalidLabelColor = new Lazy<UIColor>(() => DimOrangeColor);
        public static UIColor InvalidLabelColor => invalidLabelColor.Value;

        /// <summary>
        /// Gray label color
        /// </summary>
        static readonly Lazy<UIColor> grayLabelColor = new Lazy<UIColor>(() => UIColor.FromRGB(127, 127, 127));
        public static UIColor GrayLabelColor => grayLabelColor.Value;

        /// <summary>
        /// Text fields border color
        /// </summary>
        static readonly Lazy<UIColor> textFieldBorderColor = new Lazy<UIColor>(() => UIColor.FromRGBA(0, 0, 0, 25));
        public static UIColor TextFieldBorderColor => textFieldBorderColor.Value;

        /// <summary>
        /// Text fields hint text color
        /// </summary>
        static readonly Lazy<UIColor> textFieldHintColor = new Lazy<UIColor>(() => UIColor.FromRGBA(0, 0, 0, 100));
        public static UIColor TextFieldHintColor => textFieldHintColor.Value;

        /// <summary>
        /// Text fields text color
        /// </summary>
        static readonly Lazy<UIColor> textFieldTextColor = new Lazy<UIColor>(() => UIColor.Black);
        public static UIColor TextFieldTextColor => textFieldTextColor.Value;

        /// <summary>
        /// Dim orange color
        /// </summary>
        static readonly Lazy<UIColor> dimOrangeColor = new Lazy<UIColor>(() => UIColor.FromRGB(254, 201, 95));
        public static UIColor DimOrangeColor => dimOrangeColor.Value;

        /// <summary>
        /// Dark gray color
        /// </summary>
        static readonly Lazy<UIColor> darkGrayColor = new Lazy<UIColor>(() => UIColor.FromRGB(0x73, 0x81, 0x82));
        public static UIColor DarkGrayColor => darkGrayColor.Value;

        /// <summary>
        /// Keypad border color
        /// </summary>
        static readonly Lazy<UIColor> keypadBorderColor = new Lazy<UIColor>(() => UIColor.FromRGBA(90, 111, 138, 128));
        public static UIColor KeypadBorderColor => keypadBorderColor.Value;

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
        /// Returns the default font with a certain size
        /// </summary>
        public static CGColor ToCGColor(this UIColor color)
        {
            return color.CGColor;
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