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
	partial class FoldersViewController : BaseViewController
    {
		public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }

        private FoldersViewModel _foldersViewModel;

        public FoldersViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            FoldersTableView.TableFooterView = new UIView(CGRect.Empty);

            var insets = new UIEdgeInsets(0, 0, Theme.StatusBarHeight + NavigationController.NavigationBarHeight(), 0);
            FoldersTableView.ContentInset = insets;
            FoldersTableView.ScrollIndicatorInsets = insets;

            _foldersViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, NavigationController);

            await _foldersViewModel.GetFoldersListAsync();

            FoldersTableView.Source = new FoldersSource(_foldersViewModel.FoldersList, SelectedExtension, SelectedAccount, NavigationController);
            FoldersTableView.ReloadData();

            View.AddSubview(FoldersTableView);

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = "x" + SelectedExtension.ExtensionNumber;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => NavigationController.PopViewController(true), "Extensions", true), false);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            base.ViewWillAppear(animated);
	    }
    }
}