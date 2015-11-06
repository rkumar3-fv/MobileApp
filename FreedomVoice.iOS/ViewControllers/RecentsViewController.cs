using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using UIKit;
using Xamarin.Contacts;
using System.Text.RegularExpressions;
using CoreGraphics;
using FreedomVoice.iOS.Helpers;
using AddressBook;
using AddressBookUI;
using Contacts;
using ContactsUI;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class RecentsViewController : UIViewController
	{        
        private RecentsSource _recentSource;

        private List<Contact> _contactList;

        private CallerIdView CallerIdView { get; set; }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

        public RecentsViewController (IntPtr handle) : base (handle) { }

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
                _recentSource.OnRecentInfoClicked += IOS9TableSourceOnRecentInfoClicked;
            else
                _recentSource.OnRecentInfoClicked += IOS8TableSourceOnRecentInfoClicked;

            var addressBook = new Xamarin.Contacts.AddressBook();
            if (!await addressBook.RequestPermission())
            {
                //TODO: Do we need this alert?
                new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close").Show();
                return;
            }

            _contactList = addressBook.ToList();

            base.ViewDidLoad();
        }

        private void IOS9TableSourceOnRecentInfoClicked(Recent recent)
        {
            NSError error;
            var store = new CNContactStore();
            CNContact contact = null;
            CNContactFetchRequest request = new CNContactFetchRequest(CNContactKey.PhoneNumbers);
            store.EnumerateContacts(request, out error, (item, cursor) => {
                if (item.PhoneNumbers.Any(p=> Regex.Replace(p.Value.StringValue, @"[^\d]", "") == Regex.Replace(recent.PhoneNumber, @"[^\d]", "")))                
                    contact = item;
            });

            if (contact != null)
            {
                CNContactFetchRequest keysToFetch = new CNContactFetchRequest(CNContactViewController.DescriptorForRequiredKeys);

                var fetchKeys = new NSArray[] { keysToFetch.KeysToFetch };
                contact = store.GetUnifiedContact(contact.Identifier, fetchKeys, out error);

                try {                    
                    var view = CNContactViewController.FromContact(contact);
                    //PresentViewController(view, true, null);
                    NavigationController.PushViewController(view, true);
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }                
        }

        private void IOS8TableSourceOnRecentInfoClicked(Recent recent)
        {
            NSError error;
            var addressBook = ABAddressBook.Create(out error);
            var person = addressBook.GetPerson(Int32.Parse(recent.ContactId));

            if (person == null)
                return;

            var pvc = new ABPersonViewController()
            {
                DisplayedPerson = person,
                AllowsEditing = false
            };

            NavigationController.PushViewController(pvc, true);
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

            foreach(var item in res)
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

            NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, args) => { RecentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (s, args) => { ClearAll(); }), true);
        }

        private void ReturnToRecentsView()
        {
            RecentsTableView.SetEditing(false, true);

            NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);
        }

        private UIBarButtonItem GetEditButton()
        {
            return new UIBarButtonItem("Edit", UIBarButtonItemStyle.Plain, (s, args) => { SetEditMode(); });
        }

        private void ClearAll()
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            alertController.AddAction(UIAlertAction.Create("Clear All Recents", UIAlertActionStyle.Destructive, a => {
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
            e.TableView.InsertRows(new[] { NSIndexPath.FromRowSection (e.TableView.NumberOfRowsInSection(0), 0) }, UITableViewRowAnimation.Fade);

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