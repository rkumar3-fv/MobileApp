using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
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

            var headerHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, headerHeight, 0);
            MessagesTableView.ContentInset = insets;
            MessagesTableView.ScrollIndicatorInsets = insets;

            var frame = new CGRect(15, 0, Theme.ScreenBounds.Width - 30, 30);
            _noMessagesLabel = new UILabel(frame)
            {
                Text = "No messages",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Center = new CGPoint(View.Center.X, View.Center.Y - headerHeight),
                Hidden = true
            };

            View.Add(_noMessagesLabel);

            _messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);

            await _messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount);

            _messagesList = _messagesViewModel.MessagesList;

            var source = new MessagesSource(_messagesList, SelectedAccount, NavigationController);
            source.OnRowCallbackClick += OnSourceRowCallbackClick;
            source.OnRowViewFaxClick += OnSourceRowViewFaxClick;
            MessagesTableView.Source = source;

            MessagesTableView.ReloadData();

            CheckIfTableEmpty(MessagesCount);

            base.ViewDidLoad();
        }

        private void OnSourceRowCallbackClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            var selectedMessagePhoneNumber = e.SelectedMessage.SourceNumber;

            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, selectedMessagePhoneNumber, NavigationController);
            AddRecent(selectedMessagePhoneNumber);
        }

        private static void AddRecent(string phoneNumber)
        {
            MainTabBarInstance.Recents.Add(new Recent(string.Empty, phoneNumber, DateTime.Now));
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