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
        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

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

            var source = new MessagesSource(_messagesList, SelectedAccount, NavigationController);
            source.OnRowCallbackClick += OnSourceRowCallbackClick;
            source.OnRowViewFaxClick += OnSourceRowViewFaxClick;
            source.OnRowDeleteMessageClick += OnSourceRowDeleteMessageClick;
            MessagesTableView.Source = source;

            MessagesTableView.ReloadData();

            CheckIfTableEmpty(MessagesCount);

            base.ViewDidLoad();
        }

        private void OnSourceRowDeleteMessageClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var selectedMessage = _messagesList[e.IndexPath.Row];
            if (selectedMessage == null) return;

            if (selectedMessage.Folder == "Trash")
            {

            }
            else
            {
                e.TableView.DeselectRow(e.IndexPath, false);
                MessagesTableView.BeginUpdates();
                _messagesList.Remove(selectedMessage);
                MessagesTableView.DeleteRows(new[] { e.IndexPath }, UITableViewRowAnimation.Fade);
                MessagesTableView.EndUpdates();
            }
        }

        private async void OnSourceRowViewFaxClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var faxViewController = AppDelegate.GetViewController<FaxViewController>();
            faxViewController.FilePath = e.FilePath;
            faxViewController.SelectedFolderTitle = SelectedFolder.DisplayName;
            faxViewController.OnBackButtonClicked += async (s, args) => await DismissViewControllerAsync(true);

            var navigationController = new UINavigationController(faxViewController);
            await PresentViewControllerAsync(navigationController, true);
        }

        private void OnSourceRowCallbackClick(object sender, ExpandedCellButtonClickEventArgs e)
        {            
            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, e.SelectedMessage.SourceNumber, NavigationController);
        }

        public override void ViewWillAppear(bool animated)
	    {
            NavigationItem.Title = SelectedFolder.DisplayName;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => NavigationController.PopViewController(true), "x" + SelectedExtension.ExtensionNumber), false);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

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