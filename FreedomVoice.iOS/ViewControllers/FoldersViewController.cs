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
	partial class FoldersViewController : BaseViewController
    {
		public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
	    public List<FolderWithCount> FoldersList { private get; set; }

	    private UITableView _foldersTableView;
	    private FoldersSource _foldersSource;

        private bool _justLoaded;

        public FoldersViewController(IntPtr handle) : base(handle) { }

        public override void ViewDidLoad()
        {
            _justLoaded = true;
            var insets = new UIEdgeInsets(0, 0, Theme.StatusBarHeight + NavigationController.NavigationBarHeight() + Theme.TabBarHeight, 0);

            _foldersSource = new FoldersSource(FoldersList, SelectedExtension, SelectedAccount, NavigationController);
            _foldersTableView = new UITableView
            {
                Frame = Theme.ScreenBounds,
                TableFooterView = new UIView(CGRect.Empty),
                Source = _foldersSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_foldersTableView);

            base.ViewDidLoad();
        }

	    public override async void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = "x" + SelectedExtension.ExtensionNumber;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => NavigationController.PopViewController(true), "Extensions", true), false);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

	        if (!_justLoaded)
	        {
	            var foldersViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, NavigationController);
	            await foldersViewModel.GetFoldersListAsync();

	            FoldersList = foldersViewModel.FoldersList;
	            _foldersSource.Folders = FoldersList;
                _foldersTableView.ReloadData();
	        }
	        _justLoaded = false;

            base.ViewWillAppear(animated);
	    }
    }
}