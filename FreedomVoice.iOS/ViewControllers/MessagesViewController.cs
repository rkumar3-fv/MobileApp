using System;
using FreedomVoice.iOS.Entities;
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

        public MessagesViewController (IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            var MessagesTableView = new UITableView();

            var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName);
            await messagesViewModel.GetMessagesListAsync();

            MessagesTableView.Source = new MessagesSource(messagesViewModel.MessagesList, SelectedExtension, SelectedAccount, SelectedFolder, NavigationController);

            View.AddSubview(MessagesTableView);

            base.ViewDidLoad();
        }
    }
}