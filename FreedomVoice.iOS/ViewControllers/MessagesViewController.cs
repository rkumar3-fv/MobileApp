using System;
using System.Collections.Generic;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.ViewModels;
using UIKit;
using Foundation;
using WatchKit;

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

            var source = new MessagesSource(_messagesList, SelectedExtension, SelectedAccount, SelectedFolder, NavigationController);
            source.OnRowCallbackClick += Source_OnRowCallbackClick;
            source.OnRowViewFaxClick += Source_OnRowViewFaxClick;
            source.OnRowDeleteMessageClick += Source_OnRowDeleteMessageClick;
            MessagesTableView.Source = source;

            MessagesTableView.ReloadData();

            NavigationItem.SetLeftBarButtonItems(Appearance.GetBackButtonWithArrow(NavigationController, false, "x" + SelectedExtension.ExtensionNumber), false);

            CheckIfTableEmpty(MessagesCount);

            base.ViewDidLoad();
        }

        private void Source_OnRowDeleteMessageClick(object sender, ExpandedCellButtonClickEventArgs e)
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

        private void Source_OnRowViewFaxClick(object sender, ExpandedCellButtonClickEventArgs e)
        {
            var barTintColor = MainTabBarInstance.NavigationController.NavigationBar.BarTintColor;
            var leftBarButtonItems = MainTabBarInstance.NavigationItem.LeftBarButtonItems;
            var rightBarButtonItem = MainTabBarInstance.NavigationItem.RightBarButtonItem;
            var title = MainTabBarInstance.Title;

            var webView = new UIWebView(new CGRect(0, 60, View.Frame.Width, View.Frame.Height));
            MainTabBarInstance.View.AddSubview(webView);
            MainTabBarInstance.TabBar.Hidden = true;
            MainTabBarInstance.NavigationController.NavigationBar.BarTintColor = UIColor.FromRGBA(35, 53, 77, 155);
            webView.LoadRequest(new NSUrlRequest(new NSUrl(e.FilePath, false)));
            webView.ScalesPageToFit = true;
            MainTabBarInstance.Title = "Fax";

            var backBtn = Appearance.GetBackButtonWithArrow(this,  () => {
                MainTabBarInstance.NavigationController.NavigationBar.BarTintColor = barTintColor;
                MainTabBarInstance.TabBar.Hidden = false;
                webView.Hidden = true;
                MainTabBarInstance.Title = title;
                MainTabBarInstance.NavigationItem.SetLeftBarButtonItems(leftBarButtonItems, true);
                MainTabBarInstance.NavigationItem.SetRightBarButtonItem(rightBarButtonItem, true);
            });


            MainTabBarInstance.NavigationItem.SetLeftBarButtonItems(backBtn, true);
            MainTabBarInstance.NavigationItem.RightBarButtonItem = null;
        }

        private void Source_OnRowCallbackClick(object sender, ExpandedCellButtonClickEventArgs e)
        {            
            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, e.SelectedMessage.SourceNumber, NavigationController);
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