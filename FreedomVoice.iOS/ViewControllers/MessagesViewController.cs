using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
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
        protected override string PageName => "Messages Screen";

        public Account SelectedAccount { private get; set; }
        public ExtensionWithCount SelectedExtension { private get; set; }
        public FolderWithCount SelectedFolder { private get; set; }
	    private List<Message> MessagesList { get; set; }

	    private UITableView _messagesTableView;
	    private MessagesSource _messagesSource;

	    private NSTimer _updateTimer;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

	    public MessagesViewController(IntPtr handle) : base(handle)
	    {
            MessagesList = new List<Message>();
        }

        public override async void ViewDidLoad()
        {
            var headerHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, Theme.TabBarHeight, 0);

            var noMessagesLabel = new UILabel
            {
                Frame = new CGRect(15, Theme.ScreenBounds.Height / 2 - headerHeight - 15, Theme.ScreenBounds.Width - 30, 30),
                Text = "No messages",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center
            };

            _messagesSource = new MessagesSource(MessagesList, SelectedAccount, this);
            _messagesSource.OnRowCallbackClick += OnSourceRowCallbackClick;
            _messagesSource.OnRowViewFaxClick += OnSourceRowViewFaxClick;

            _messagesTableView = new UITableView
            {
                Frame = new CGRect(0, 0, Theme.ScreenBounds.Width, Theme.ScreenBounds.Height - headerHeight),
                TableFooterView = new UIView(CGRect.Empty),
                Source = _messagesSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_messagesTableView);

            var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, MainTabBarInstance.Contacts);
            messagesViewModel.OnUnauthorizedResponse += (sender, args) => OnUnauthorizedError();
            await messagesViewModel.GetMessagesListAsync();

            MessagesList = messagesViewModel.MessagesList;
            _messagesSource.Messages = MessagesList;

            _messagesTableView.BackgroundView = noMessagesLabel;
            _messagesTableView.ReloadData();

            base.ViewDidLoad();
        }

        private async void OnUnauthorizedError()
        {
            var appDelegate = UIApplication.SharedApplication.Delegate as AppDelegate;
            if (appDelegate != null)
                await appDelegate.LoginWithStoredCredentials();

            UpdateMessagesTable();
        }

        private async void OnSourceRowCallbackClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            var selectedMessagePhoneNumber = e.SelectedMessage.SourceNumber;

            if (await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, selectedMessagePhoneNumber, this))
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

            _updateTimer = NSTimer.CreateRepeatingScheduledTimer(UserDefault.PoolingInterval, delegate { UpdateMessagesTable(); });

            base.ViewWillAppear(animated);
        }

	    public override void ViewWillDisappear(bool animated)
	    {
            AppDelegate.ResetAudioPlayer();
            AppDelegate.CancelActiveDownload();

            _messagesSource.ReloadSelectedRow(_messagesTableView);

            _updateTimer.Invalidate();
        }

        private async void UpdateMessagesTable()
        {
            var needToReloadTable = false;

            await Task.Run(async () => 
            {
                var messagesViewModel = new MessagesViewModel(SelectedAccount.PhoneNumber, SelectedExtension.ExtensionNumber, SelectedFolder.DisplayName, MainTabBarInstance.Contacts);

                await messagesViewModel.GetMessagesListAsync(true);
                if (messagesViewModel.IsErrorResponseReceived) return;

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

                needToReloadTable = true;
            });

            if (needToReloadTable)
                _messagesTableView.ReloadData();
        }
    }
}