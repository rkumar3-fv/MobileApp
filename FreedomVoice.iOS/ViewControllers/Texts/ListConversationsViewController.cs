using System;
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
using FreedomVoice.iOS.Views.Shared;
using UIKit;

namespace FreedomVoice.iOS.ViewControllers.Texts
{
    public partial class ListConversationsViewController : BaseViewController
    {
        protected override string PageName => "Texts";

        private UILabel _noItemsLabel;
        private UITableView _tableView;
        private CallerIdView _callerIdView;
        private LineView _lineView;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;
        private ConversationsPresenter _presenter;
        private ContactNameProvider _contactNameProvider;

        public ListConversationsViewController(IntPtr handle) : base(handle)
        {
            _noItemsLabel = new UILabel
            {
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(28),
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

            _callerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers())
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };


            _lineView = new LineView(CGRect.Empty)
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            _SetupViews();
            _SetupConstraints();
            _SetupData();
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

            var createButton = new UIBarButtonItem(UIBarButtonSystemItem.Compose, (sender, e) => { });

            NavigationItem.RightBarButtonItem = createButton;
        }

        private void _SetupConstraints()
        {
            _noItemsLabel.CenterYAnchor.ConstraintEqualTo(View.CenterYAnchor).Active = true;
            _noItemsLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor, 15).Active = true;
            _noItemsLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor, -15).Active = true;
            _noItemsLabel.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _callerIdView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _callerIdView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _callerIdView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _callerIdView.HeightAnchor.ConstraintEqualTo(40).Active = true;

            _lineView.TopAnchor.ConstraintEqualTo(_callerIdView.BottomAnchor).Active = true;
            _lineView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _lineView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _lineView.HeightAnchor.ConstraintEqualTo(0.5f).Active = true;

            _tableView.TopAnchor.ConstraintEqualTo(_lineView.BottomAnchor).Active = true;
            _tableView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _tableView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _tableView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        private void _SetupData()
        {
            CallerIdEvent.CallerIdChanged += UpdateCallerId;

            var provider = ServiceContainer.Resolve<IContactNameProvider>();
            provider.ContactsUpdated += ProviderOnContactsUpdated;

            _presenter = new ConversationsPresenter()
            {
                PhoneNumber = _callerIdView.SelectedNumber.PhoneNumber
            };
            _presenter.ItemsChanged += (sender, args) =>
            {
                _noItemsLabel.Hidden = _presenter.Items.Count > 0;
            };
            _tableView.Source = new ConversationsSource(_presenter, NavigationController, _tableView);
            _tableView.ReloadData();
        }

        private void ProviderOnContactsUpdated(object sender, EventArgs e)
        {
            _tableView.ReloadData();
            _contactNameProvider.ContactsUpdated -= ProviderOnContactsUpdated;
        }

        private void UpdateCallerId(object sender, EventArgs args)
        {
            var selectedPresentationNumber = (args as CallerIdEventArgs)?.SelectedPresentationNumber;
            if (selectedPresentationNumber == null)
                return;
            _presenter.PhoneNumber = selectedPresentationNumber.PhoneNumber;
            _presenter.ReloadAsync();
        }
    }
}

