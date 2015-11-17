using System;
using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Utilities
{
    public static class Theme
    {
        private static readonly bool _iPhone6 = AppDelegate.CurrentDeviceName == "iPhone 6" || AppDelegate.CurrentDeviceName == "iPhone 6S";
        private static readonly bool _iPhone6Plus = AppDelegate.CurrentDeviceName == "iPhone 6 Plus" || AppDelegate.CurrentDeviceName == "iPhone 6S Plus";

        #region Backgrounds

        /// <summary>
        /// Background for Login page
        /// </summary>
        static readonly Lazy<UIColor> loginBackground = new Lazy<UIColor>(() => UIColor.FromPatternImage(LoginBackgroundImage));
        public static UIColor LoginBackground => loginBackground.Value;

        #endregion

        #region Images

        /// <summary>
        /// Login page background image
        /// </summary>
        static readonly Lazy<UIImage> loginBackgroundImage = new Lazy<UIImage>(() => UIImage.FromFile($"Login-{(_iPhone6Plus ? "736h" : _iPhone6 ? "667h" : "568h")}.png"));
        public static UIImage LoginBackgroundImage => loginBackgroundImage.Value;

        /// <summary>
        /// Splash screen background image
        /// </summary>
        static readonly Lazy<UIImage> splashScreenImage = new Lazy<UIImage>(() => UIImage.FromFile($"Default-{(_iPhone6Plus ? "736h" : _iPhone6 ? "667h" : "568h")}.png"));
        public static UIImage SplashScreenImage => splashScreenImage.Value;

        /// <summary>
        /// Keypad Dial image
        /// </summary>
        private static readonly Lazy<UIImage> keypadDialImage = new Lazy<UIImage>(() => UIImage.FromFile($"keypad_call{(_iPhone6 ? "_big" : "")}.png"));
        public static UIImage KeypadDialImage => keypadDialImage.Value;

        public static UIImage LoginLogoImage(bool keyboardVisible) => UIImage.FromFile($"logo_freedomvoice{(!_iPhone6 && !_iPhone6Plus && keyboardVisible ? "_small" : "")}_white.png");

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
        static readonly Lazy<UIColor> buttonColor = new Lazy<UIColor>(() => OrangeColor);
        public static UIColor ButtonColor => buttonColor.Value;

        /// <summary>
        /// Blue button color
        /// </summary>
        static readonly Lazy<UIColor> barButtonColor = new Lazy<UIColor>(() => BlueColor);
        public static UIColor BarButtonColor => barButtonColor.Value;

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
        static readonly Lazy<UIColor> loginPageTextFieldTextColor = new Lazy<UIColor>(() => WhiteColor);
        public static UIColor LoginPageTextFieldTextColor => loginPageTextFieldTextColor.Value;

        /// <summary>
        /// Invalid text field border color
        /// </summary>
        static readonly Lazy<UIColor> invalidTextFieldBorderColor = new Lazy<UIColor>(() => RedColor);
        public static UIColor InvalidTextFieldBorderColor => invalidTextFieldBorderColor.Value;

        /// <summary>
        /// Invalid label color
        /// </summary>
        static readonly Lazy<UIColor> loginInvalidLabelColor = new Lazy<UIColor>(() => WhiteColor);
        public static UIColor LoginInvalidLabelColor => loginInvalidLabelColor.Value;

        /// <summary>
        /// Invalid label color
        /// </summary>
        static readonly Lazy<UIColor> invalidLabelColor = new Lazy<UIColor>(() => RedColor);
        public static UIColor InvalidLabelColor => invalidLabelColor.Value;

        /// <summary>
        /// Invalid label color
        /// </summary>
        static readonly Lazy<UIColor> invalidLabelBackgroundColor = new Lazy<UIColor>(() => RedColor);
        public static UIColor InvalidLabelBackgroundColor => invalidLabelBackgroundColor.Value;

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
        static readonly Lazy<UIColor> textFieldTextColor = new Lazy<UIColor>(() => BlackColor);
        public static UIColor TextFieldTextColor => textFieldTextColor.Value;

        /// <summary>
        /// Background for SearchBar and PickerView Toolbar
        /// </summary>
        static readonly Lazy<UIColor> barBackgroundColor = new Lazy<UIColor>(() => UIColor.FromRGB(230, 234, 238));
        public static UIColor BarBackgroundColor => barBackgroundColor.Value;

        /// <summary>
        /// Black color
        /// </summary>
        static readonly Lazy<UIColor> blackColor = new Lazy<UIColor>(() => UIColor.Black);
        public static UIColor BlackColor => blackColor.Value;

        /// <summary>
        /// White color
        /// </summary>
        static readonly Lazy<UIColor> whiteColor = new Lazy<UIColor>(() => UIColor.White);
        public static UIColor WhiteColor => whiteColor.Value;

        /// <summary>
        /// Gray color
        /// </summary>
        static readonly Lazy<UIColor> grayColor = new Lazy<UIColor>(() => UIColor.FromRGB(127, 127, 127));
        public static UIColor GrayColor => grayColor.Value;

        /// <summary>
        /// Blue color
        /// </summary>
        static readonly Lazy<UIColor> blueColor = new Lazy<UIColor>(() => UIColor.FromRGB(3, 138, 193));
        public static UIColor BlueColor => blueColor.Value;

        /// <summary>
        /// Dim orange color
        /// </summary>
        static readonly Lazy<UIColor> orangeColor = new Lazy<UIColor>(() => UIColor.FromRGB(240, 170, 30));
        public static UIColor OrangeColor => orangeColor.Value;

        /// <summary>
        /// Dark gray color
        /// </summary>
        static readonly Lazy<UIColor> darkGrayColor = new Lazy<UIColor>(() => UIColor.FromRGB(73, 81, 82));
        public static UIColor DarkGrayColor => darkGrayColor.Value;

        /// <summary>
        /// Dark gray color
        /// </summary>
        static readonly Lazy<UIColor> redColor = new Lazy<UIColor>(() => UIColor.FromRGB(240, 78, 61));
        public static UIColor RedColor => redColor.Value;


        /// <summary>
        /// Keypad border color
        /// </summary>
        static readonly Lazy<UIColor> keypadBorderColor = new Lazy<UIColor>(() => UIColor.FromRGBA(90, 111, 138, 128));
        public static UIColor KeypadBorderColor => keypadBorderColor.Value;

        #endregion

        #region Dimensions

        public static CGRect ScreenBounds => UIScreen.MainScreen.Bounds;

        public static nfloat StatusBarHeight => UIApplication.SharedApplication.StatusBarFrame.Height;

        public static nfloat TabBarHeight => 49;

        public static nfloat BackButtonWidth(bool wideLabel)
        {
            if (AppDelegate.SystemVersion == 9)
                return wideLabel ? 105 : 93;

            return wideLabel ? 110 : 98;
        }

        public static nfloat LogoImageTopPadding(bool keyboardVisible) => !_iPhone6 && !_iPhone6Plus && keyboardVisible ? 10 : 35;

        public static nfloat LogoImageWidth(bool keyboardVisible = false) => !_iPhone6 && !_iPhone6Plus && keyboardVisible ? 184 : 263;

        public static nfloat LogoImageHeight(bool keyboardVisible = false) => !_iPhone6 && !_iPhone6Plus && keyboardVisible ? 29 : 42;

        public static nfloat WelcomeLabelTopPadding(bool keyboardVisible = false) => _iPhone6Plus ? (keyboardVisible ? 32 : 118) : _iPhone6 ? (keyboardVisible ? 15 : 94) : (keyboardVisible ? 4 : 57);

        public static UIFont WelcomeLabelFont(bool keyboardVisible) => UIFont.SystemFontOfSize((!_iPhone6 && !_iPhone6Plus && keyboardVisible ? 24 : 36), UIFontWeight.Thin);

        public static nfloat WelcomeLabelHeight(bool keyboardVisible = false) => !_iPhone6 && !_iPhone6Plus && keyboardVisible ? 18 : 30;

        public static nfloat UsernameTextFieldTopPadding(bool keyboardVisible = false) => !_iPhone6 && !_iPhone6Plus && keyboardVisible ? 10 : 28;

        public static nfloat PasswordTextFieldPadding => _iPhone6Plus ? 33 : 27;

        public static nfloat LoginValidationLabelTopPadding => _iPhone6Plus ? 17 : 10;

        public static nfloat BackButtonLabelWidth => AppDelegate.SystemVersion == 9 ? 86 : 91;

        public static nfloat KeypadButtonDiameter => _iPhone6 || _iPhone6Plus ? 75 : 65;

        public static nfloat KeypadDistanceX => _iPhone6 || _iPhone6Plus ? 28 : 16;

        public static nfloat KeypadDistanceY => _iPhone6 || _iPhone6Plus ? 15 : 8;

        public static nfloat KeypadWidth => KeypadButtonDiameter * 3 + KeypadDistanceX * 2;

        public static nfloat KeypadHeight => (KeypadButtonDiameter + KeypadDistanceY) * 4;

        public static nfloat KeypadTopPadding => _iPhone6Plus ? 44 : _iPhone6 ? 17 : 0;

        public static nfloat KeypadDialButtonDiameter => KeypadButtonDiameter - 3;

        public static nfloat NavigationBarHeight(this UINavigationController navigationController)
        {
            return navigationController.NavigationBar?.Frame.Size.Height ?? 0;
        }

        #endregion

        /// <summary>
        /// Apply UIAppearance to this application, this is iOS's version of "styling"
        /// </summary>
        public static void Apply()
        {
            UIActivityIndicatorView.Appearance.Color = IndicatorColor;

            UINavigationBar.Appearance.TintColor = WhiteColor;
            UINavigationBar.Appearance.SetTitleTextAttributes(new UITextAttributes { TextColor = WhiteColor });

            //var navigationBarImage = AppDelegate.CurrentDeviceName == iPhone6 || AppDelegate.CurrentDeviceName == iPhone6S ? "navbar_wide.png" : "navbar.png";
            //UINavigationBar.Appearance.SetBackgroundImage(UIImage.FromFile(navigationBarImage), UIBarMetrics.Default);
            UINavigationBar.Appearance.SetBackgroundImage(UIImage.FromFile("navbar.png"), UIBarMetrics.Default);
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