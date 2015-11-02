using System;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.ViewModels;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MessagesViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
        public FolderWithCount SelectedFolder { private get; set; }

        private MessagesViewModel _messagesViewModel;

        public MessagesViewController (IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            MessagesTableView.TableFooterView = new UIView(CGRect.Empty);

            _messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);

            await _messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount);

            MessagesTableView.Source = new MessagesSource(_messagesViewModel.MessagesList, SelectedExtension, SelectedAccount, SelectedFolder, NavigationController);
            MessagesTableView.ReloadData();

            View.AddSubview(MessagesTableView);

            NavigationItem.SetLeftBarButtonItems(Appearance.GetBackButtonWithArrow(NavigationController, false, "x" + SelectedExtension.ExtensionNumber), false);

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = SelectedFolder.DisplayName;

            base.ViewWillAppear(animated);
	    }
    }
}