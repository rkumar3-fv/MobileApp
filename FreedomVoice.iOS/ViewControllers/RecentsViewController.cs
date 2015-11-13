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
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.Views;
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

        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

        public RecentsViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            RecentsTableView.TableFooterView = new UIView(CGRect.Empty);

            CallerIdView = new CallerIdView(new RectangleF(0, 0, (float)View.Frame.Width, 40), MainTabBarInstance.GetPresentationNumbers());

            var recentLineView = new RecentLineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)View.Frame.Width, 0.5f));
            View.AddSubviews(CallerIdView, recentLineView);

            RecentsTableView.TableHeaderView = CallerIdView;

            _recentSource = new RecentsSource(GetRecentsOrdered());
            RecentsTableView.Source = _recentSource;

            _recentSource.OnRowSelected += TableSourceOnRowSelected;
            _recentSource.OnRowDeleted += TableSourceOnRowDeleted;

            if (UIDevice.CurrentDevice.CheckSystemVersion(9, 0))
                _recentSource.OnRecentInfoClicked += TableSourceOnRecentInfoClicked;
            else
                _recentSource.OnRecentInfoClicked += DeprecatedTableSourceOnRecentInfoClicked;

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
            return _contactList?.FirstOrDefault(c => c.Phones.Any(p => Regex.Replace(p.Number, @"[^\d]", "") == Regex.Replace(number, @"[^\d]", "")));
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

        private static void ClearRecent()
        {
            MainTabBarInstance.Recents.Clear();
        }

        private static void RemoveRecent(Recent recent)
        {
            MainTabBarInstance.Recents.Remove(recent);
        }

        public override void ViewWillAppear(bool animated)
        {
            NavigationItem.Title = "Recents";
            NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            _recentSource.SetRecents(GetRecentsUpdatedAndOrdered());
            RecentsTableView.ReloadData();

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        private void SetEditMode()
        {
            RecentsTableView.SetEditing(true, true);

            NavigationItem.SetRightBarButtonItem(Appearance.GetPlainBarButton("Done", (s, args) => { RecentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            NavigationItem.SetLeftBarButtonItem(Appearance.GetPlainBarButton("Clear", (s, args) => { ClearAll(); }), true);
        }

        private void ReturnToRecentsView()
        {
            RecentsTableView.SetEditing(false, true);

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
                _recentSource.SetRecents(GetRecentsOrdered());
                RecentsTableView.ReloadData();
            }));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, a => { ReturnToRecentsView(); }));

            PresentViewController(alertController, true, null);
        }

        private void TableSourceOnRowSelected(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            RecentsTableView.BeginUpdates();

            var newRecent = (Recent)recent.Clone();
            newRecent.DialDate = DateTime.Now;
            AddRecent(newRecent);

            _recentSource.SetRecents(GetRecentsOrdered());
            e.TableView.InsertRows(new[] { NSIndexPath.FromRowSection(e.TableView.NumberOfRowsInSection(0), 0) }, UITableViewRowAnimation.Fade);

            RecentsTableView.EndUpdates();
            RecentsTableView.ReloadRows(e.TableView.IndexPathsForVisibleRows, UITableViewRowAnimation.None);

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;
            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, newRecent.PhoneNumber, NavigationController);
        }

        private void TableSourceOnRowDeleted(object sender, RecentsSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var recent = GetRecentsOrdered()[e.IndexPath.Row];
            if (recent == null) return;

            RecentsTableView.BeginUpdates();

            RemoveRecent(recent);

            _recentSource.SetRecents(GetRecentsOrdered());

            RecentsTableView.DeleteRows(new[] { e.IndexPath }, UITableViewRowAnimation.Fade);
            RecentsTableView.EndUpdates();
        }
    }
}