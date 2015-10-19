using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Contacts;
using CoreGraphics;
using FreedomVoice.iOS.Helpers;

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

            searchBar.CancelButtonClicked += (sender, e) => {                
                Reset();
            };


            searchBar.TextChanged += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.SearchText))                                    
                    Reset();                                    
                else                
                    Search();                
            };
            

            TableView.TableHeaderView = searchBar;
            TableView.SectionIndexBackgroundColor = UIColor.Clear;

            var containerWidth = 80;
            var containerHeight = 20;
            
            var frame = new CGRect(View.Frame.Width / 2 - containerWidth / 2, View.Frame.Height / 2 - containerHeight / 2, containerWidth, containerHeight);            
            lblNoResults = new UILabel(frame);            
            lblNoResults.Text = "No Result";
            lblNoResults.MinimumFontSize = 36f;
            lblNoResults.TextColor = UIColor.FromRGB(127,127,127);
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
            searchBar.Text = string.Empty;
            bInSearchMode = false;
            var book = new Xamarin.Contacts.AddressBook();
            people = (from b in book select b).ToList();
            TableView.ReloadData();
            CheckResult();
        }

        void CheckResult()
        {
            if (people.Count == 0)
            {
                lblNoResults.Alpha = 1f;
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                lblNoResults.Alpha = 0f;
                TableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }

            searchBar.ShowsCancelButton = !String.IsNullOrEmpty(searchBar.Text);
        }
        

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {

            var pe = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[indexPath.Section]) select p).ToList()[indexPath.Row];
            Console.WriteLine($"pe.Phones.Count(){pe.Phones.Count()}");

            if (PhoneCapability.IsAirplaneMode())
            {
                UIAlertController okAlertController = UIAlertController.Create(null, "Airplane Mode must be turned off to make calls from the FreedomVoice app.", UIAlertControllerStyle.Alert);
                okAlertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => {
                    var settingsString = UIApplication.OpenSettingsUrlString;                    
                    var url = new NSUrl(settingsString);
                    UIApplication.SharedApplication.OpenUrl(url);
                }));
                okAlertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

                PresentViewController(okAlertController, true, null);
                return;
            }

            if (!PhoneCapability.IsCellularEnabled())                
            {
                UIAlertController okAlertController = UIAlertController.Create(null, "Your device does not appear to support making cellular voice calls.", UIAlertControllerStyle.Alert);
                okAlertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, a => {}));
                PresentViewController(okAlertController, true, null);
                return;
            }
        }        
    }
}
