using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class ExtensionsViewController : BaseViewController
    {
        protected override string PageName => "Extensions Screen";

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        private static Account SelectedAccount => MainTabBarInstance.SelectedAccount;

        private List<ExtensionWithCount> ExtensionsList { get; set; }

        private UITableView _extensionsTableView;
        private ExtensionsSource _extensionsSource;

        public ExtensionsViewController(IntPtr handle) : base(handle)
        {
            ExtensionsList = new List<ExtensionWithCount>();
        }

        public override void ViewDidLoad()
        {
            var insets = new UIEdgeInsets(0, 0, Theme.StatusBarHeight + NavigationController.NavigationBarHeight() + Theme.TabBarHeight, 0);

            _extensionsSource = new ExtensionsSource(ExtensionsList, NavigationController);
            _extensionsTableView = new UITableView
            {
                Frame = Theme.ScreenBounds,
                TableFooterView = new UIView(CGRect.Empty),
                Source = _extensionsSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_extensionsTableView);

            base.ViewDidLoad();
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationItem.Title = SelectedAccount.FormattedPhoneNumber;

            if (!MainTabBarInstance.IsRootController)
                NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => MainTabBarInstance.NavigationController.PopViewController(true), "Accounts"), false);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            var extensionsViewModel = new ExtensionsViewModel(SelectedAccount);
            await extensionsViewModel.GetExtensionsListAsync();

            ExtensionsList = extensionsViewModel.ExtensionsList;
            _extensionsSource.Extensions = ExtensionsList;
            _extensionsTableView.ReloadData();
        }

        public override void ViewDidDisappear(bool animated)
        {
            ExtensionsList = new List<ExtensionWithCount>();
            _extensionsSource.Extensions = ExtensionsList;
            _extensionsTableView.ReloadData();
        }
    }
}