using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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

        private List<ExtensionWithCount> ExtensionsList { get; set; }

        private UITableView _extensionsTableView;
        private ExtensionsSource _extensionsSource;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;
        private static Account SelectedAccount => MainTabBarInstance.SelectedAccount;

        private NSTimer _updateTimer;

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
            extensionsViewModel.OnUnauthorizedResponse += (sender, args) => OnUnauthorizedError();
            await extensionsViewModel.GetExtensionsListAsync();

            ExtensionsList = extensionsViewModel.ExtensionsList;
            _extensionsSource.Extensions = ExtensionsList;
            _extensionsTableView.ReloadData();

            _updateTimer = NSTimer.CreateRepeatingScheduledTimer(UserDefault.PoolingInterval, delegate { UpdateExtensionsTable(); });
        }

        private async void OnUnauthorizedError()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate != null)
                await appDelegate.LoginWithStoredCredentials();

            UpdateExtensionsTable();
        }

        private async void UpdateExtensionsTable()
        {
            var needToReloadTable = false;

            await Task.Run(async () =>
            {
                var extensionsViewModel = new ExtensionsViewModel(SelectedAccount);
                await extensionsViewModel.GetExtensionsListAsync(true);
                if (extensionsViewModel.IsErrorResponseReceived) return;

                ExtensionsList = extensionsViewModel.ExtensionsList;
                _extensionsSource.Extensions = ExtensionsList;

                needToReloadTable = true;
            });

            if (needToReloadTable)
                _extensionsTableView.ReloadData();
        }

        public override void ViewDidDisappear(bool animated)
        {
            ExtensionsList = new List<ExtensionWithCount>();
            _extensionsSource.Extensions = ExtensionsList;
            _extensionsTableView.ReloadData();

            _updateTimer.Invalidate();
        }
    }
}