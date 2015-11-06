using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class FaxViewController : BaseViewController
    {
        public string FilePath { private get; set; }
        public string SelectedFolderTitle { private get; set; }

        public EventHandler OnBackButtonClicked;

        private UIWebView _webView;

        public FaxViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            NavigationController.NavigationBar.SetBackgroundImage(ImageFromColor(UIColor.FromRGBA(35, 53, 77, 155), new CGRect(0, 0, Theme.ScreenBounds.Width, 64)), UIBarMetrics.Default);
            NavigationController.NavigationBar.ShadowImage = new UIImage();
            NavigationController.NavigationBar.Translucent = true;
            NavigationController.View.BackgroundColor = UIColor.Clear;

            _webView = new UIWebView(new CGRect(0, 0, View.Frame.Width, View.Frame.Height)) { ScalesPageToFit = true };
            _webView.LoadRequest(new NSUrlRequest(new NSUrl(FilePath, false)));

            View.Add(_webView);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Fax";
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow(OnBackButtonClicked, SelectedFolderTitle), true);

            base.ViewWillAppear(animated);
        }

        private static UIImage ImageFromColor(UIColor color, CGRect frame)
        {
            UIGraphics.BeginImageContextWithOptions(frame.Size, false, 0);
            color.SetFill();
            UIGraphics.RectFill(frame);
            var image = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return image;
        }
    }
}