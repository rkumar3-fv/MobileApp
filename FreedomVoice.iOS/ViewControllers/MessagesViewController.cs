using System;
using System.Collections.Generic;
using System.Linq;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewModels;
using GoogleAnalytics.iOS;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MessagesViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
        public FolderWithCount SelectedFolder { private get; set; }
	    private List<Message> MessagesList { get; set; }

	    private int MessagesCount => MessagesList.Count;

        private UILabel _noMessagesLabel;
	    private UITableView _messagesTableView;
	    private MessagesSource _messagesSource;

	    private NSTimer _updateTimer;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

	    public MessagesViewController(IntPtr handle) : base(handle)
	    {
            MessagesList = new List<Message>();

            GAI.SharedInstance.DefaultTracker.Set(GAIConstants.ScreenName, "Messages Screen");
            GAI.SharedInstance.DefaultTracker.Send(GAIDictionaryBuilder.CreateScreenView().Build());
        }

        public override async void ViewDidLoad()
        {
            var headerHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, headerHeight + Theme.TabBarHeight, 0);

            _messagesSource = new MessagesSource(MessagesList, SelectedAccount, NavigationController);
            _messagesSource.OnRowCallbackClick += OnSourceRowCallbackClick;
            _messagesSource.OnRowViewFaxClick += OnSourceRowViewFaxClick;

            _messagesTableView = new UITableView
            {
                Frame = Theme.ScreenBounds,
                TableFooterView = new UIView(CGRect.Empty),
                Source = _messagesSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_messagesTableView);

            var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);
            await messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount);

            MessagesList = messagesViewModel.MessagesList;
            _messagesSource.Messages = MessagesList;
            _messagesTableView.ReloadData();

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

            CheckIfTableEmpty(MessagesCount);

            base.ViewDidLoad();
        }

        private async void OnSourceRowCallbackClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            var selectedMessagePhoneNumber = e.SelectedMessage.SourceNumber;

            if (await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, selectedMessagePhoneNumber, NavigationController))
                AddRecent(selectedMessagePhoneNumber);
        }

        private static void AddRecent(string phoneNumber)
        {
            MainTabBarInstance.AddRecent(new Recent(string.Empty, phoneNumber, DateTime.Now));
        }

        private void OnSourceRowViewFaxClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            NavigationItem.LeftBarButtonItem = null;

            var faxViewController = AppDelegate.GetViewController<FaxViewController>();
            faxViewController.FilePath = e.FilePath;
            faxViewController.SelectedFolderTitle = SelectedFolder.DisplayName;
            faxViewController.OnBackButtonClicked += (s, args) => DismissViewController(true, () => { });

            var faxController = new UINavigationController(faxViewController);
            PresentViewController(faxController, true, () => { });
        }

        public override void ViewWillAppear(bool animated)
	    {
            Theme.Apply();

            NavigationItem.Title = SelectedFolder.DisplayName;
            NavigationItem.SetLeftBarButtonItems(Appearance.GetBarButtonWithArrow((s, args) => NavigationController.PopViewController(true), "x" + SelectedExtension.ExtensionNumber), false);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            _updateTimer = NSTimer.CreateRepeatingScheduledTimer(UserDefault.PoolingInterval, delegate { UpdateMessageTable(); });

            base.ViewWillAppear(animated);
	    }

        public override void ViewDidDisappear(bool animated)
        {
            AppDelegate.ResetAudioPlayer();
            _updateTimer.Invalidate();
        }

        private async void UpdateMessageTable()
        {
            var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);
            await messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount, true);

            var recievedMessages = messagesViewModel.MessagesList;

            var messagesToAdd = recievedMessages.Where(message => !MessagesList.Exists(m => m.Id == message.Id)).ToList();
            var messagesToRemove = MessagesList.Where(message => !recievedMessages.Exists(m => m.Id == message.Id)).ToList();

            if (messagesToAdd.Count == 0 && messagesToRemove.Count == 0)
                return;

            var selectedMessage = _messagesSource.SelectedRowIndexPath != null ? MessagesList[_messagesSource.SelectedRowIndexPath.Row] : null;

            var selectedMessageIndex = recievedMessages.FindIndex(m => m.Id == selectedMessage?.Id);

            MessagesList = recievedMessages;
            _messagesSource.Messages = MessagesList;
            _messagesSource.SelectedRowIndexPath = _messagesSource.DeletedRowIndexPath = selectedMessageIndex != -1 ? NSIndexPath.FromRowSection(selectedMessageIndex, 0) : null;

            _messagesTableView.ReloadData();
        }

        private void CheckIfTableEmpty(int messagesCount)
        {
            if (messagesCount == 0)
            {
                _noMessagesLabel.Hidden = false;
                _messagesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noMessagesLabel.Hidden = true;
                _messagesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }
    }
}