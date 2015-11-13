using System;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class FaxViewController : BaseViewController
    {
        public string FilePath { private get; set; }
        public string SelectedFolderTitle { private get; set; }

        public EventHandler OnBackButtonClicked;

        public FaxViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            EdgesForExtendedLayout = UIRectEdge.None;

            var webView = new UIWebView(new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height - Theme.StatusBarHeight - NavigationController.NavigationBarHeight()));
            View.AddSubview(webView);

            webView.LoadRequest(new NSUrlRequest(new NSUrl(FilePath, false)));
            webView.ScalesPageToFit = true;

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Fax";

            var backgroundImage = Appearance.GetImageFromColor(UIColor.FromRGB(35, 53, 77), new CGSize(View.Bounds.Width, Theme.StatusBarHeight + NavigationController.NavigationBarHeight()));
            NavigationController.NavigationBar.SetBackgroundImage(backgroundImage, UIBarMetrics.Default);

            var backButtonTitle = ((NSString)SelectedFolderTitle).StringSize(UIFont.SystemFontOfSize(17, UIFontWeight.Medium)).Width > 86 ? "Back" : SelectedFolderTitle;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow(OnBackButtonClicked, backButtonTitle, true), true);

            base.ViewWillAppear(animated);
        }
    }
}