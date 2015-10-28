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
    partial class ExtensionsViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }

        LoadingOverlay _loadingOverlay;
        ExtensionsViewModel _extensionsViewModel;

        public ExtensionsViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            ExtensionsTableView.TableFooterView = new UIView(CGRect.Empty);

            _extensionsViewModel = new ExtensionsViewModel(SelectedAccount.PhoneNumber);
            _extensionsViewModel.IsBusyChanged += OnIsBusyChanged;
            _extensionsViewModel.IsBusy = true;

            await _extensionsViewModel.GetExtensionsListAsync();

            ExtensionsTableView.Source = new ExtensionsSource(_extensionsViewModel.ExtensionsList, SelectedAccount, NavigationController);
            ExtensionsTableView.ReloadData();

            _extensionsViewModel.IsBusy = false;

            View.AddSubview(ExtensionsTableView);

            base.ViewDidLoad();
        }

        private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!IsViewLoaded)
                return;

            if (_extensionsViewModel.IsBusy)
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