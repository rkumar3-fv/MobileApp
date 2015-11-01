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

namespace FreedomVoice.iOS.ViewControllers
{
    //TODO: Clean class from commented code
    partial class RecentsViewController : UIViewController
	{        
        private UIBarButtonItem _tempLeftButton;
        private UIBarButtonItem _tempRightButton;
        private RecentsSource _recentSource;

        private List<Contact> _contactList;

	    private CallerIdView CallerIdView { get; set; }

        //private UIViewController MainTab => ParentViewController.ParentViewController;
        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

        public RecentsViewController (IntPtr handle) : base (handle) { }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            RecentsTableView.TableFooterView = new UIView(CoreGraphics.CGRect.Empty);

            //CallerIdView = new CallerIdView(new RectangleF(0, 65, 320, 40), (MainTab as MainTabBarController)?.GetPresentationNumbers());
            CallerIdView = new CallerIdView(new RectangleF(0, 65, 320, 40), MainTabBarInstance.GetPresentationNumbers());

            var recentLineView = new RecentLineView(new RectangleF(0, 40, 320, 0.5f));
            View.AddSubviews(CallerIdView, recentLineView);

            RecentsTableView.TableHeaderView = CallerIdView;

            _recentSource = new RecentsSource(GetRecentsOrdered());
            RecentsTableView.Source = _recentSource;

            _recentSource.OnRowSelected += TableSourceOnRowSelected;
            _recentSource.OnRowDeleted += TableSourceOnRowDeleted;

            var addressBook = new Xamarin.Contacts.AddressBook();
            if (!await addressBook.RequestPermission())
            {                
                new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close").Show();
                return;
            }

            _contactList = addressBook.ToList();
        }

        private Contact FindContactByNumber(string number)
        {
            //try
            //{
            //    List<Contact> res = _contactList?.Where(c => c.Phones.Any(p => Regex.Replace(p.Number, @"[^\d]", "") == Regex.Replace(number, @"[^\d]", ""))).ToList();
            //    return res?.First();
            //}
            //catch { }

            //return null;

            return _contactList?.FirstOrDefault(c => c.Phones.Any(p => Regex.Replace(p.Number, @"[^\d]", "") == Regex.Replace(number, @"[^\d]", "")));
        }

	    private static List<Recent> GetRecentsOrdered()
        {            
            return GetRecents().OrderByDescending(o => o.DialDate).ToList();
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

        private static List<Recent> GetRecents()
        {         
            //return (MainTab as MainTabBarController)?.Recents;
            return MainTabBarInstance.Recents;
        }   
        
        private static void AddRecent(Recent recent)
        {
            MainTabBarInstance.Recents.Add(recent);
        }

	    private static void ClearRecent()
        {
            GetRecents().Clear();
        }

        private static void RemoveRecent(Recent recent)
        {
            GetRecents().Remove(recent);
        }

        public override void ViewWillDisappear(bool animated)
        {
            //MainTab.NavigationItem.SetLeftBarButtonItem(_tempLeftButton, true);
            //MainTab.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);
            MainTabBarInstance.NavigationItem.SetLeftBarButtonItem(_tempLeftButton, true);
            MainTabBarInstance.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);

            base.ViewWillDisappear(animated);
        }

        public override void ViewWillAppear(bool animated)
        {
            //_tempLeftButton = MainTab.NavigationItem.LeftBarButtonItem;
            //_tempRightButton = MainTab.NavigationItem.RightBarButtonItem;
            _tempLeftButton = MainTabBarInstance.NavigationItem.LeftBarButtonItem;
            _tempRightButton = MainTabBarInstance.NavigationItem.RightBarButtonItem;

            //MainTab.Title = "Recents";
            //MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            MainTabBarInstance.Title = "Recents";
            MainTabBarInstance.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);

            _recentSource.SetRecents(GetRecentsUpdatedAndOrdered());
            RecentsTableView.ReloadData();

            //PresentationNumber selectedNumber = (MainTab as MainTabBarController)?.GetSelectedPresentationNumber();
            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        private void SetEditMode()
        {
            RecentsTableView.SetEditing(true, true);
            //MainTab.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, args) => { RecentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            //MainTab.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (s, args) => { ClearAll(); }), true);
            MainTabBarInstance.NavigationItem.SetRightBarButtonItem(new UIBarButtonItem("Done", UIBarButtonItemStyle.Plain, (s, args) => { RecentsTableView.ReloadData(); ReturnToRecentsView(); }), true);
            MainTabBarInstance.NavigationItem.SetLeftBarButtonItem(new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, (s, args) => { ClearAll(); }), true);
        }

        private void ReturnToRecentsView()
        {
            RecentsTableView.SetEditing(false, true);
            //MainTab.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            //MainTab.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);
            MainTabBarInstance.NavigationItem.SetLeftBarButtonItem(GetEditButton(), true);
            MainTabBarInstance.NavigationItem.SetRightBarButtonItem(_tempRightButton, true);
        }

        private UIBarButtonItem GetEditButton()
        {
            return new UIBarButtonItem("Edit", UIBarButtonItemStyle.Plain, (s, args) => { SetEditMode(); });
        }

        private void ClearAll()
        {
            var alertController = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            alertController.AddAction(UIAlertAction.Create("Clear All Recents", UIAlertActionStyle.Default, a => {
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
            e.TableView.InsertRows(new[] { NSIndexPath.FromRowSection (e.TableView.NumberOfRowsInSection (0), 0) }, UITableViewRowAnimation.Fade);

            RecentsTableView.EndUpdates();
            RecentsTableView.ReloadRows(e.TableView.IndexPathsForVisibleRows, UITableViewRowAnimation.None);
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