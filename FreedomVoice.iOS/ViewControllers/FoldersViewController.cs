using System;
using System.Collections.Generic;
using System.Diagnostics;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class FoldersViewController : BaseViewController
    {
        protected override string PageName => "Folders Screen";

        public bool IsSingleExtension { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
	    private List<FolderWithCount> FoldersList { get; set; }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;
        private static Account SelectedAccount => MainTabBarInstance.SelectedAccount;

        private UITableView _foldersTableView;

        private FoldersSource _foldersSource;

	    private nfloat _insetsHeight;

	    public FoldersViewController(IntPtr handle) : base(handle)
	    {
            FoldersList = new List<FolderWithCount>();
        }

        public override void ViewDidLoad()
        {
            InitializeTableView();

            base.ViewDidLoad();
        }

        public override async void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "x" + SelectedExtension.ExtensionNumber;

            if (IsSingleExtension)
            {
                if (!MainTabBarInstance.IsRootController)
                    NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => MainTabBarInstance.NavigationController.PopViewController(true), "Accounts"), false);
            }
            else
                NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => NavigationController.PopViewController(true), "Extensions", true), false);

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            var foldersViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber);

            var watcher = Stopwatch.StartNew();
            await foldersViewModel.GetFoldersListAsync();
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.Request, "GetFolders", "", watcher.ElapsedMilliseconds);

            FoldersList = foldersViewModel.FoldersList;
            _foldersSource.Folders = FoldersList;
            _foldersTableView.ReloadData();

            base.ViewWillAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            FoldersList = new List<FolderWithCount>();
            _foldersSource.Folders = FoldersList;
            _foldersTableView.ReloadData();
        }

        private void InitializeTableView()
        {
            _insetsHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight() + Theme.TabBarHeight;
            var insets = new UIEdgeInsets(0, 0, _insetsHeight, 0);

            _foldersSource = new FoldersSource(FoldersList, SelectedExtension, SelectedAccount, NavigationController);
	        _foldersTableView = new UITableView
	        {
	            Frame = Theme.ScreenBounds,
	            TableFooterView = new UIView(CGRect.Empty),
	            Source = _foldersSource,
	            ContentInset = insets,
	            ScrollIndicatorInsets = insets
	        };
	        View.AddSubview(_foldersTableView);
	    }
    }
}