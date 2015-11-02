using System;
using System.Collections.Generic;
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

        private MessagesViewModel _messagesViewModel;
        private List<Message> _messagesList;

	    private int MessagesCount => _messagesList.Count;

        private UILabel _noMessagesLabel;

        public MessagesViewController (IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            MessagesTableView.TableFooterView = new UIView(CGRect.Empty);

            var frame = new CGRect(15, 0, Theme.ScreenBounds.Width - 30, 30);
            _noMessagesLabel = new UILabel(frame)
            {
                Text = "No messages",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Center = View.Center,
                Hidden = true
            };

            View.Add(_noMessagesLabel);

            _messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);

            await _messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount);

            _messagesList = _messagesViewModel.MessagesList;

            MessagesTableView.Source = new MessagesSource(_messagesList, SelectedExtension, SelectedAccount, SelectedFolder, NavigationController);
            MessagesTableView.ReloadData();

            NavigationItem.SetLeftBarButtonItems(Appearance.GetBackButtonWithArrow(NavigationController, false, "x" + SelectedExtension.ExtensionNumber), false);

            CheckIfTableEmpty(MessagesCount);

            base.ViewDidLoad();
        }

	    public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = SelectedFolder.DisplayName;

            base.ViewWillAppear(animated);
	    }

        private void CheckIfTableEmpty(int messagesCount)
        {
            if (messagesCount == 0)
            {
                _noMessagesLabel.Hidden = false;
                MessagesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noMessagesLabel.Hidden = true;
                MessagesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }
    }
}