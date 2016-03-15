using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using AddressBookUI;
using Contacts;
using ContactsUI;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views.Shared;
using UIKit;
using ContactsHelper = FreedomVoice.iOS.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class RecentsViewController : BaseViewController
    {
        protected override string PageName => "Recents Screen";

        private RecentsSource _recentSource;

        private CallerIdView CallerIdView { get; set; }
        private UITableView _recentsTableView;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public RecentsViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            CallerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers());
            View.AddSubview(CallerIdView);

            var headerHeight = CallerIdView.Frame.Height + Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, Theme.TabBarHeight, 0);

            var noItemsLabel = new UILabel
            {
                Frame = new CGRect(15, Theme.ScreenBounds.Height / 2 - headerHeight - 15, Theme.ScreenBounds.Width - 30, 30),
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center
            };

            _recentSource = new RecentsSource(Recents.GetRecentsOrdered(), this);

            _recentsTableView = new UITableView
            {
                Frame = new CGRect(0, CallerIdView.Frame.Height, Theme.ScreenBounds.Width, Theme.ScreenBounds.Height - headerHeight),
                TableFooterView = new UIView(CGRect.Empty),
                BackgroundView = noItemsLabel,
                Source = _recentSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_recentsTableView);

            if (AppDelegate.SystemVersion == 9)
                _recentSource.OnRecentInfoClicked += TableSourceOnRecentInfoClicked;
            else
                _recentSource.OnRecentInfoClicked += DeprecatedTableSourceOnRecentInfoClicked;

            var recentLineView = new LineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)Theme.ScreenBounds.Width, 0.5f));
            View.AddSubviews(recentLineView);

            await ContactsHelper.GetContactsListAsync();

            base.ViewDidLoad();
        }

        private void TableSourceOnRecentInfoClicked(Recent recent)
        {
            CNContact contact = null;

            var store = new CNContactStore();
            var request = new CNContactFetchRequest(CNContactKey.PhoneNumbers);

            NSError error;
            store.EnumerateContacts(request, out error, (item, cursor) =>
            {
                if (item.PhoneNumbers.Any(p => ContactsHelper.NormalizePhoneNumber(p.Value.StringValue) == ContactsHelper.NormalizePhoneNumber(recent.PhoneNumber)))
                    contact = item;
            });

            if (contact == null) return;

            contact = store.GetUnifiedContact(contact.Identifier, new[] { CNContactViewController.DescriptorForRequiredKeys }, out error);

            var viewController = CNContactViewController.FromContact(contact);
            viewController.AllowsEditing = false;
            viewController.AllowsActions = false;

            NavigationController.PushViewController(viewController, true);
        }

        private void DeprecatedTableSourceOnRecentInfoClicked(Recent recent)
        {
            var addressBook = ContactsHelper.GetAddressBook();

            var person = addressBook?.GetPerson(int.Parse(recent.ContactId));

            if (person == null)
                return;

            var viewController = new ABPersonViewController { DisplayedPerson = person, AllowsEditing = false, AllowsActions = false };

            NavigationController.PushViewController(viewController, true);
        }

        private static List<Recent> GetRecentsUpdatedAndOrdered()
        {
            var recents = Recents.GetRecentsOrdered();

            foreach (var item in recents)
            {
                var foundContact = ContactsHelper.FindContactByNumber(item.PhoneNumber);
                if (foundContact == null) continue;

                item.Title = foundContact.DisplayName;
                item.ContactId = foundContact.Id;
            }

            return recents;
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Recents";

            SetEditMode(false);

            _recentSource.SetRecents(GetRecentsUpdatedAndOrdered());
            _recentsTableView.ReloadData();

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            AppDelegate.EnableUserInteraction(UIApplication.SharedApplication);

            base.ViewDidAppear(animated);
        }

        public override void ViewWillDisappear(bool animated)
        {
            _recentsTableView?.SetEditing(false, animated);

            base.ViewWillDisappear(animated);
        }

        private void SetEditMode(bool editing)
        {
            _recentsTableView.SetEditing(editing, true);

            if (editing)
            {
                NavigationItem.SetRightBarButtonItem(Appearance.GetPlainBarButton("Done", (s, args) => { _recentsTableView.ReloadData(); SetEditMode(false); }), true);
                NavigationItem.SetLeftBarButtonItem(Appearance.GetPlainBarButton("Clear", (s, args) => { ClearAll(); }), true);
            }
            else
            {
                NavigationItem.SetLeftBarButtonItem(Appearance.GetPlainBarButton("Edit", (s, args) => { SetEditMode(true); }), true);
                NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);
            }
        }

        private void ClearAll()
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            alertController.AddAction(UIAlertAction.Create("Clear All Recents", UIAlertActionStyle.Destructive, a =>
            {
                Recents.ClearRecents();
                SetEditMode(false);
                _recentSource.SetRecents(new List<Recent>());
                _recentsTableView.ReloadData();
            }));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { SetEditMode(false); }));

            PresentViewController(alertController, true, null);
        }
    }
}