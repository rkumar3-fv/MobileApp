using System;
using Foundation;
using FreedomVoice.iOS.Helpers;
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
            NavigationController.NavigationBar.BarTintColor = UIColor.FromRGB(90, 111, 138);
            EdgesForExtendedLayout = UIRectEdge.None;

            var webView = new UIWebView(View.Bounds);
            View.AddSubview(webView);

            webView.LoadRequest(new NSUrlRequest(new NSUrl(FilePath, false)));
            webView.ScalesPageToFit = true;

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Fax";

            var backButtonTitle = ((NSString)SelectedFolderTitle).StringSize(UIFont.SystemFontOfSize(17, UIFontWeight.Medium)).Width > 86 ? "Back" : SelectedFolderTitle;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow(OnBackButtonClicked, backButtonTitle, true), true);

            base.ViewWillAppear(animated);
        }
    }
}