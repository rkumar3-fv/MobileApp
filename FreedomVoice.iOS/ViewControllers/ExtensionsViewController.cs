using System;
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
        public ExtensionsViewController(IntPtr handle) : base(handle) { }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        private static Account SelectedAccount => MainTabBarInstance.SelectedAccount;

        private UITableView _extensionsTableView;
        private ExtensionsSource _extensionsSource;
        private bool _justLoaded;

        public override void ViewDidLoad()
        {
            _justLoaded = true;
            var insets = new UIEdgeInsets(0, 0, Theme.StatusBarHeight + NavigationController.NavigationBarHeight() + Theme.TabBarHeight, 0);

            _extensionsSource = new ExtensionsSource(MainTabBarInstance.ExtensionsList, SelectedAccount, NavigationController);
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
            NavigationItem.Title = SelectedAccount.FormattedPhoneNumber;

            if (!MainTabBarInstance.IsRootController)
                NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => MainTabBarInstance.NavigationController.PopViewController(true), "Accounts"), true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            if (!_justLoaded)
            {
                var mainTabBarViewModel = new MainTabBarViewModel(SelectedAccount, NavigationController);
                await mainTabBarViewModel.GetExtensionsListAsync();

                MainTabBarInstance.ExtensionsList = mainTabBarViewModel.ExtensionsList;
                _extensionsSource.Extensions = mainTabBarViewModel.ExtensionsList;
                _extensionsTableView.ReloadData();
            }
            _justLoaded = false;

            base.ViewWillAppear(animated);
        }
    }
}