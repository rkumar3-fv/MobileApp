using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using AddressBook;
using AddressBookUI;
using Contacts;
using ContactsUI;
using CoreGraphics;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views.Shared;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class RecentsViewController : UIViewController
    {
        private RecentsSource _recentSource;

        private List<Contact> _contactList;

        private CallerIdView CallerIdView { get; set; }
        private UITableView _recentsTableView;

        private UILabel _noItemsLabel;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        private static int RecentsCount => MainTabBarInstance.Recents.Count;

        public RecentsViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            CallerIdView = new CallerIdView(new RectangleF(0, 0, (float)Theme.ScreenBounds.Width, 40), MainTabBarInstance.GetPresentationNumbers());
            View.AddSubview(CallerIdView);

            _recentSource = new RecentsSource(GetRecentsOrdered());

            var insets = new UIEdgeInsets(0, 0, Theme.StatusBarHeight + NavigationController.NavigationBarHeight(), 0);

            _recentsTableView = new UITableView
            {
                Frame = new CGRect(0, CallerIdView.Frame.Height, Theme.ScreenBounds.Width, Theme.ScreenBounds.Height - Theme.TabBarHeight - CallerIdView.Frame.Height),
                TableFooterView = new UIView(CGRect.Empty),
                Source = _recentSource,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_recentsTableView);

            _recentSource.OnRowSelected += TableSourceOnRowSelected;
            _recentSource.OnRowDeleted += TableSourceOnRowDeleted;

            if (AppDelegate.SystemVersion == 9)
                _recentSource.OnRecentInfoClicked += TableSourceOnRecentInfoClicked;
            else
                _recentSource.OnRecentInfoClicked += DeprecatedTableSourceOnRecentInfoClicked;

            var recentLineView = new LineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)Theme.ScreenBounds.Width, 0.5f));
            View.AddSubviews(recentLineView);

            var frame = new CGRect(15, 0, Theme.ScreenBounds.Width - 30, 30);
            _noItemsLabel = new UILabel(frame)
            {
                Text = "No Items",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Center = new CGPoint(View.Center.X, _recentsTableView.Center.Y - Theme.TabBarHeight),
                Hidden = true
            };
            View.Add(_noItemsLabel);

            CheckResult(RecentsCount);

            _contactList = await AppDelegate.GetContactsListAsync();

            base.ViewDidLoad();
        }

        private void TableSourceOnRecentInfoClicked(Recent recent)
        {
            var store = new CNContactStore();
            NSError error;

            CNContact contact = null;

            var request = new CNContactFetchRequest(CNContactKey.PhoneNumbers);
            store.EnumerateContacts(request, out error, (item, cursor) =>
            {
                if (item.PhoneNumbers.Any(p => Regex.Replace(p.Value.StringValue, @"[^\d]", "") == Regex.Replace(recent.PhoneNumber, @"[^\d]", "")))
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
            NSError error;
            var addressBook = ABAddressBook.Create(out error);
            var person = addressBook.GetPerson(int.Parse(recent.ContactId));

            if (person == null)
                return;

            var viewController = new ABPersonViewController { DisplayedPerson = person, AllowsEditing = false, AllowsActions = false };

            NavigationController.PushViewController(viewController, true);
        }

        private Contact FindContactByNumber(string number)
        {
            return _contactList?.FirstOrDefault(c => c.Phones.Any(p => DataFormatUtils.NormalizePhone(p.Number) == DataFormatUtils.NormalizePhone(number)));
        }

        private static List<Recent> GetRecentsOrdered()
        {
            return MainTabBarInstance.Recents.OrderByDescending(o => o.DialDate).ToList();
        }

        private List<Recent> GetRecentsUpdatedAndOrdered()
        {
            List<Recent> res = GetRecentsOrdered();

            foreach (var item in res)
            {
                if (!string.IsNullOrEmpty(item.ContactId))
                    continue;

                Contact findContact = FindContactByNumber(item.PhoneNumber);
                if (findContact == null) continue;

                item.Title = findContact.DisplayName;
                item.ContactId = findContact.Id;
            }
            return res;
        }

        private static void AddRecent(Recent recent)
        {
            MainTabBarInstance.Recents.Add(recent);
        }

        private void ClearRecent()
        {
            MainTabBarInstance.Recents.Clear();
            CheckResult(RecentsCount);
        }

        private void RemoveRecent(Recent recent)
        {
            MainTabBarInstance.Recents.Remove(recent);
            CheckResult(RecentsCount);
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Recents";
            NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            _recentSource.SetRecents(GetRecentsUpdatedAndOrdered());
            _recentsTableView.ReloadData();

            CheckResult(RecentsCount);

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            _recentsTableView.SetEditing(false, false);

            base.ViewDidDisappear(animated);
        }

        private void SetEditMode()
        {
            _recentsTableView.SetEditing(true, true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetPlainBarButton("Done", (s, args) => { _recentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            NavigationItem.SetLeftBarButtonItem(Appearance.GetPlainBarButton("Clear", (s, args) => { ClearAll(); }), true);
        }

        private void ReturnToRecentsView()
        {
            _recentsTableView.SetEditing(false, true);

            NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);
        }

        private UIBarButtonItem GetEditButton()
        {
            return Appearance.GetPlainBarButton("Edit", (s, args) => { SetEditMode(); });
        }

        private void ClearAll()
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            alertController.AddAction(UIAlertAction.Create("Clear All Recents", UIAlertActionStyle.Destructive, a =>
            {
                ReturnToRecentsView();
                ClearRecent();
                _recentSource.SetRecents(new List<Recent>());
                _recentsTableView.ReloadData();
            }));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { ReturnToRecentsView(); }));

            PresentViewController(alertController, true, null);
        }

        private async void TableSourceOnRowSelected(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            if (!await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, recent.PhoneNumber, NavigationController)) return;

            var newRecent = (Recent)recent.Clone();
            newRecent.DialDate = DateTime.Now;
            AddRecent(newRecent);

            _recentsTableView.BeginUpdates();

            _recentSource.SetRecents(GetRecentsOrdered());
            e.TableView.InsertRows(new[] { NSIndexPath.FromRowSection(0, 0) }, UITableViewRowAnimation.Top);

            _recentsTableView.EndUpdates();
        }

        private void TableSourceOnRowDeleted(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            _recentsTableView.BeginUpdates();

            RemoveRecent(recent);

            _recentSource.SetRecents(GetRecentsOrdered());

            _recentsTableView.DeleteRows(new[] { e.IndexPath }, UITableViewRowAnimation.Fade);
            _recentsTableView.EndUpdates();
        }

        private void CheckResult(int contactsCount)
        {
            if (contactsCount == 0)
            {
                _noItemsLabel.Hidden = false;
                _recentsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noItemsLabel.Hidden = true;
                _recentsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }
    }
}