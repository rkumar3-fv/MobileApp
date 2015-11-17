using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class MessagesViewController : BaseViewController
    {
        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
        public FolderWithCount SelectedFolder { private get; set; }
        public List<Message> MessagesList { private get; set; }

	    private int MessagesCount => MessagesList.Count;

        private UILabel _noMessagesLabel;
	    private UITableView _messagesTableView;
	    private MessagesSource _messagesSource;

	    //private NSTimer _updateTimer;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public MessagesViewController (IntPtr handle) : base (handle) { }

        public override void ViewDidLoad()
        {
            var headerHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, headerHeight + Theme.TabBarHeight, 0);

            _messagesTableView = new UITableView
            {
                Frame = Theme.ScreenBounds,
                TableFooterView = new UIView(CGRect.Empty),
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };

            _messagesSource = new MessagesSource(MessagesList, SelectedAccount, NavigationController);
            _messagesSource.OnRowCallbackClick += OnSourceRowCallbackClick;
            _messagesSource.OnRowViewFaxClick += OnSourceRowViewFaxClick;
            _messagesTableView.Source = _messagesSource;

            View.Add(_messagesTableView);

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

            //_updateTimer = NSTimer.CreateRepeatingScheduledTimer(UserDefault.PoolingInterval, delegate {
            //    UpdateMessageTable();
            //});

            base.ViewWillAppear(animated);
	    }

        //public override void ViewWillDisappear(bool animated)
        //{
        //    _updateTimer.Invalidate();
        //}

        //private async void UpdateMessageTable()
        //{
        //    var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, NavigationController);
        //    await messagesViewModel.GetMessagesListAsync(SelectedFolder.MessageCount);

        //    MessagesList = messagesViewModel.MessagesList;

        //    _messagesTableView.BeginUpdates();
        //    _messagesSource.MessagesList = MessagesList;
        //    _messagesTableView.InsertRows(new[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.Top);
        //    _messagesTableView.EndUpdates();

        //    new UIAlertView(null, "Table updated", null, "Ok").Show();
        //}

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