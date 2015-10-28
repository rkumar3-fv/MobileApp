using System;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class FoldersViewController : BaseViewController
    {
		public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }

        LoadingOverlay _loadingOverlay;
        FoldersViewModel _foldersViewModel;

        public FoldersViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            //Title = SelectedExtension.ExtensionNumber.ToString();

            FoldersTableView.TableFooterView = new UIView(CGRect.Empty);

            _foldersViewModel = new FoldersViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber);
            _foldersViewModel.IsBusyChanged += OnIsBusyChanged;
            _foldersViewModel.IsBusy = true;

            await _foldersViewModel.GetFoldersListAsync();

            FoldersTableView.Source = new FoldersSource(_foldersViewModel.FoldersList, SelectedExtension, SelectedAccount, NavigationController);
            FoldersTableView.ReloadData();

            _foldersViewModel.IsBusy = false;

            View.AddSubview(FoldersTableView);

            base.ViewDidLoad();
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!IsViewLoaded)
                return;

            if (_foldersViewModel.IsBusy)
            {
                _loadingOverlay = new LoadingOverlay(Theme.ScreenBounds);
                View.Add(_loadingOverlay);
            }
            else
            {
                _loadingOverlay.Hide();
            }
        }
    }
}