using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class ExtensionsViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public List<ExtensionWithCount> ExtensionsList { private get; set; }

        public ExtensionsViewController(IntPtr handle) : base(handle) { }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

        public override void ViewDidLoad()
        {
            ExtensionsTableView.TableFooterView = new UIView(CGRect.Empty);

            ExtensionsTableView.Source = new ExtensionsSource(ExtensionsList, SelectedAccount, NavigationController);

            View.AddSubview(ExtensionsTableView);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = SelectedAccount.FormattedPhoneNumber;

            if (!MainTabBarInstance.IsRootController)
                NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => MainTabBarInstance.NavigationController.PopViewController(true), "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            base.ViewWillAppear(animated);
        }
    }
}