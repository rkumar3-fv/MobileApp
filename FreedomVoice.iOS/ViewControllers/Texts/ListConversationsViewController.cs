﻿using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using FreedomVoice.Core.Presenters;
using FreedomVoice.Core.Services;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources.Texting;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Events;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers.Texts.NewConversation;
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ListConversationsViewController : BaseViewController
    {
        private struct Appearance
        {
            public static readonly nfloat NoItemsLabelHeight = 40;
            public static readonly nfloat CallerIdViewHeight = 40;
            public static readonly nfloat SearchViewHeight = 44;
            public static readonly nfloat LeftMargin = 15;
            public static readonly nfloat RightMargin = -15;
            public static readonly nfloat SeparatorHeight = 0.5f;
            public const int NoItemsFontSize = 28;
        }
        
        protected override string PageName => "Texts";
        private UISearchBar _searchBar;

        private UILabel _noItemsLabel;
        private UITableView _tableView;
        private CallerIdView _callerIdView;
        private LineView _lineView;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;
        private ConversationsPresenter _presenter;
        private IContactNameProvider _contactNameProvider;
        private UIRefreshControl _refreshControl;

        public ListConversationsViewController(IntPtr handle) : base(handle)
        {
            _noItemsLabel = new UILabel
            {
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(Appearance.NoItemsFontSize),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Hidden = true
            };

            _tableView = new UITableView
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                TableFooterView = new UIView()
            };

            _callerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, (float)Appearance.CallerIdViewHeight), MainTabBarInstance.GetPresentationNumbers())
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };


            _lineView = new LineView(CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            
            _searchBar = new UISearchBar
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Placeholder = ContactsViewControllerTexts.Search,
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocorrectionType = UITextAutocorrectionType.No,
                BarTintColor = Theme.BarBackgroundColor
            };
            
            var contactsSearchBarCancelButton = UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar));
            var textAttributes = new UITextAttributes { Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium), TextColor = Theme.BarButtonColor };
            contactsSearchBarCancelButton.SetTitleTextAttributes(textAttributes, UIControlState.Normal);
            
            _searchBar.SearchButtonClicked += _searchButtonClicked;
            _searchBar.CancelButtonClicked += _searchCancelClicked;
            _searchBar.ShouldBeginEditing += SearchBarOnShouldBeginEditing;
            _searchBar.ShouldEndEditing += SearchBarOnShouldEndEditing;

            _refreshControl = new UIRefreshControl();
            _refreshControl.ValueChanged += _refreshControlValueChanged;

        }

        private void _searchCancelClicked(object sender, EventArgs e)
        {
            _searchBar.Text = "";
            _presenter.Query = "";
            _reloadData();
        }

        private void _searchButtonClicked(object sender, EventArgs e)
        {
            _presenter.Query = _searchBar.Text;
            _reloadData();
        }
        
        private bool SearchBarOnShouldEndEditing(UISearchBar searchBar)
        {
            _searchBar.ShowsCancelButton = false;
            return true;
        }

        private bool SearchBarOnShouldBeginEditing(UISearchBar searchBar)
        {
            _searchBar.ShowsCancelButton = true;
            return true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _SetupViews();
            _SetupConstraints();
            _SetupData();
            CallerIdEvent.CallerIdFinished += UpdateCallerId;
        }

        public override void ViewDidUnload()
        {
            base.ViewDidUnload();
            CallerIdEvent.CallerIdFinished -= UpdateCallerId;
        }
        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = PageName;
            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            ServiceContainer.Resolve<IContactNameProvider>().RequestContacts();
        }

        private void _SetupViews()
        {
            View.AddSubview(_callerIdView);
            View.AddSubview(_lineView);
            View.AddSubview(_tableView);
            View.AddSubview(_noItemsLabel);
            View.AddSubview(_searchBar);

            var createButton = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (sender, e) =>
            {
                var controller = new NewConversationViewController {CurrentPhone = _callerIdView.SelectedNumber};
                NavigationController?.PushViewController(controller, true);
            });

            NavigationItem.RightBarButtonItem = createButton;

            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                _tableView.RefreshControl = _refreshControl;
            }
            else 
            {
                _tableView.AddSubview(_refreshControl);
            }            
        }
        
        private void _SetupConstraints()
        {
            _noItemsLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _noItemsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, Appearance.LeftMargin).Active = true;
            _noItemsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, Appearance.RightMargin).Active = true;
            _noItemsLabel.HeightAnchor.ConstraintEqualTo(Appearance.NoItemsLabelHeight).Active = true;
            
            _searchBar.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _searchBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _searchBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _searchBar.HeightAnchor.ConstraintEqualTo(Appearance.SearchViewHeight).Active = true;

            _callerIdView.TopAnchor.ConstraintEqualTo(_searchBar.BottomAnchor).Active = true;
            _callerIdView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _callerIdView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _callerIdView.HeightAnchor.ConstraintEqualTo(Appearance.CallerIdViewHeight).Active = true;

            _lineView.TopAnchor.ConstraintEqualTo(_callerIdView.BottomAnchor).Active = true;
            _lineView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _lineView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _lineView.HeightAnchor.ConstraintEqualTo(Appearance.SeparatorHeight).Active = true;

            _tableView.TopAnchor.ConstraintEqualTo(_lineView.BottomAnchor).Active = true;
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;

        }

        private void _SetupData()
        {

            _contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
            _contactNameProvider.ContactsUpdated += ProviderOnContactsUpdated;

            _presenter = new ConversationsPresenter()
            {
                PhoneNumber = _callerIdView.SelectedNumber.PhoneNumber,
                AccountNumber = UserDefault.LastUsedAccount
            };
            _presenter.ServerError += (sender, args) =>
            {
                AppDelegate.ActivityIndicator.Hide();
                _refreshControl.EndRefreshing();
                UIAlertHelper.ShowAlert(this, ConversationTexts.ErrorTitle, ConversationsPresenter.DefaultError);
            };
            _presenter.ItemsChanged += (sender, args) =>
            {
                _refreshControl.EndRefreshing();
                _noItemsLabel.Hidden = _presenter.Items.Count > 0;
                AppDelegate.ActivityIndicator.Hide();
            };
            var dataSource = new ConversationsSource(_presenter, _tableView);
            dataSource.ItemDidSelected += DataSource_ItemDidSelected;
            _tableView.Source = dataSource;
            _tableView.ReloadData();
            View.AddSubview(AppDelegate.ActivityIndicator);
            AppDelegate.ActivityIndicator.SetActivityIndicatorCenter(Theme.ScreenCenter);
            AppDelegate.ActivityIndicator.Show();
            _presenter.ReloadAsync();
        }

        void _refreshControlValueChanged(object sender, EventArgs e)
        {
            _reloadData(false);
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            _refreshControl.EndRefreshing();
            _tableView.ReloadData();
            _contactNameProvider.ContactsUpdated -= ProviderOnContactsUpdated;
            AppDelegate.ActivityIndicator.Hide();
        }

        private void UpdateCallerId(object sender, EventArgs args)
        {
            var selectedPresentationNumber = (args as CallerIdEventArgs)?.SelectedPresentationNumber;
            if (selectedPresentationNumber == null)
                return;
            _presenter.PhoneNumber = selectedPresentationNumber.PhoneNumber;
            _presenter.AccountNumber = UserDefault.LastUsedAccount;
            _reloadData();
        }

        private void _reloadData(bool withIndicator = true)
        {
            View.EndEditing(true);
            if (withIndicator)
            {
                View.AddSubview(AppDelegate.ActivityIndicator);
                AppDelegate.ActivityIndicator.Show();
            }
            _presenter.ReloadAsync();
        }

        void DataSource_ItemDidSelected(object sender, EventArgs e)
        {

            if (!(e is ConversationSelectEventArgs arg))
            {
                return;
            }
            View.EndEditing(true);
            var controller = AppDelegate.GetViewController<ConversationViewController>();
            controller.ConversationId = arg.ConversationId;
            controller.CurrentPhone = _callerIdView.SelectedNumber;
            controller.Title = arg.Name;
            NavigationController.PushViewController(controller, true);
        }

        protected override void Dispose(bool disposing)
        {
            _presenter.Dispose();
            base.Dispose(disposing);
        }

    }
}

