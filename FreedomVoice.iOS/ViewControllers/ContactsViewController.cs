using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Contacts;
using CoreGraphics;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class ContactsViewController : UITableViewController
    {
        List<Contact> people;
        string cellIdentifier = "cell";
        UIRefreshControl uirc;
        string[] letters = new string[] { "A", "B", "C", "D", "E", "F", "G", "H", "I",
            "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        Dictionary<string, string> indexedTableItems;
        string[] keys;

        UISearchBar searchBar;
        private UIAlertView alert;

        bool bInSearchMode = false;
        UILabel lblNoResults;

        public ContactsViewController(IntPtr handle) : base(handle)
        {
            people = new List<Contact>();
            keys = new string[0];
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            var book = new Xamarin.Contacts.AddressBook();
            uirc = new UIRefreshControl();
            uirc.ValueChanged += (sender, e) =>
            {
                people = (from b in book select b).ToList();
                TableView.ReloadData();
                uirc.EndRefreshing();
            };
            RefreshControl = uirc;

            book.RequestPermission().ContinueWith(t =>
            {
                if (!t.Result)
                {
                    alert = new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close");
                    alert.Show();
                    return;
                }
                people = (from b in book select b).ToList();
                indexedTableItems = new Dictionary<string, string>();
                foreach (var l in letters)
                {
                    var ContactCount = (from p in people where p.DisplayName.ToUpper().StartsWith(l) select p).Count();
                    if (ContactCount > 0)
                    {
                        indexedTableItems.Add(l, l);
                    }
                }
                keys = indexedTableItems.Keys.ToArray();
                TableView.ReloadData();
                CheckResult();
            }, TaskScheduler.FromCurrentSynchronizationContext());


            searchBar = new UISearchBar();
            searchBar.Placeholder = "Search";
            searchBar.SizeToFit();
            searchBar.AutocorrectionType = UITextAutocorrectionType.No;
            searchBar.AutocapitalizationType = UITextAutocapitalizationType.None;
            searchBar.SearchButtonClicked += (sender, e) =>
            {
                Search();
            };


            searchBar.TextChanged += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.SearchText))
                {
                    searchBar.Text = string.Empty;
                    Reset();
                }
                else
                {
                    Search();
                }
            };

            TableView.TableHeaderView = searchBar;


            var frame = new CGRect(20, 50, 300, 30);
            lblNoResults = new UILabel(frame);            
            lblNoResults.Text = "No Result";
            lblNoResults.Alpha = 0f;
            View.Add(lblNoResults);
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var count = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[section].ToUpper()) select p).Count();
            return count;
        }
        public override string[] SectionIndexTitles(UITableView tableView)
        {
            if (bInSearchMode)
                return null;

            return keys;
        }
        public override nint NumberOfSections(UITableView tableView)
        {
            return keys.Length;
        }
        public override string TitleForHeader(UITableView tableView, nint section)
        {

            if (bInSearchMode)
                return null;

            return keys[section];
        }
        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return null;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    var pe = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[indexPath.Section]) select p).ToList()[indexPath.Row];
                    people.Remove(pe);
                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    break;
                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false;
        }
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {
            var pe = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[indexPath.Section]) select p).ToList()[indexPath.Row];
            return "Delete (" + pe.DisplayName + ")";
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);
            if (cell == null)
                cell = new UITableViewCell(UITableViewCellStyle.Subtitle, cellIdentifier);

            var pe = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[indexPath.Section]) select p).ToList()[indexPath.Row];

            cell.TextLabel.Text = pe.DisplayName;
            cell.DetailTextLabel.Text = pe.Nickname;
            return cell;
        }

        void Search()
        {
            bInSearchMode = true;
            var peList = (from p in people where p.DisplayName.ToUpper().StartsWith(searchBar.Text.ToUpper()) select p).ToList();
            people = peList;
            TableView.ReloadData();
            CheckResult();
        }

        void Reset()
        {
            bInSearchMode = false;
            var book = new Xamarin.Contacts.AddressBook();
            people = (from b in book select b).ToList();
            TableView.ReloadData();
            CheckResult();
        }

        void CheckResult()
        {            
            if (people.Count == 0)                            
                lblNoResults.Alpha = 1f;
            else                            
                lblNoResults.Alpha = 0f;            
        }
    }
}
