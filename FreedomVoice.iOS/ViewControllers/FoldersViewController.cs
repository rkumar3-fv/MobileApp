using System;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
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

            _foldersViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, NavigationController);

            await _foldersViewModel.GetFoldersListAsync();

            FoldersTableView.Source = new FoldersSource(_foldersViewModel.FoldersList, SelectedExtension, SelectedAccount, NavigationController);
            FoldersTableView.ReloadData();

            View.AddSubview(FoldersTableView);

            NavigationItem.SetLeftBarButtonItems(Appearance.GetBackButtonWithArrow(NavigationController, false, "Extensions"), false);

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = "x" + SelectedExtension.ExtensionNumber;

            base.ViewWillAppear(animated);
	    }
    }
}