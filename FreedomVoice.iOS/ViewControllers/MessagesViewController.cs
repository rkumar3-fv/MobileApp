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
	partial class MessagesViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
        public FolderWithCount SelectedFolder { private get; set; }

        LoadingOverlay _loadingOverlay;
        MessagesViewModel _messagesViewModel;

        public MessagesViewController (IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            MessagesTableView.TableFooterView = new UIView(CGRect.Empty);

            _messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName);
            _messagesViewModel.IsBusyChanged += OnIsBusyChanged;
            _messagesViewModel.IsBusy = true;

            await _messagesViewModel.GetMessagesListAsync();

            MessagesTableView.Source = new MessagesSource(_messagesViewModel.MessagesList, SelectedExtension, SelectedAccount, SelectedFolder, NavigationController);
            MessagesTableView.ReloadData();

            _messagesViewModel.IsBusy = false;

            View.AddSubview(MessagesTableView);

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = SelectedFolder.DisplayName;

            base.ViewWillAppear(animated);
	    }

	    private void OnIsBusyChanged(object sender, EventArgs e)
        {
            if (!IsViewLoaded)
                return;

            if (_messagesViewModel.IsBusy)
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