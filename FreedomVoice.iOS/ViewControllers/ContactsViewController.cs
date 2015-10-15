using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Foundation;
using UIKit;
using Xamarin.Contacts;

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
            uirc.ValueChanged += (sender, e) => {
                people = (from b in book select b).ToList();
                this.TableView.ReloadData();
                uirc.EndRefreshing();
            };
            RefreshControl = uirc;

            book.RequestPermission().ContinueWith(t => {
                if (!t.Result)
                {
                    Console.WriteLine("Permission denied by user or manifest");
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
                this.TableView.ReloadData();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            var count = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[section].ToUpper()) select p).Count();
            return count;
        }
        public override string[] SectionIndexTitles(UITableView tableView)
        {
            return keys;
        }
        public override nint NumberOfSections(UITableView tableView)
        {
            return keys.Length;
        }
        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return keys[section];
        }
        public override string TitleForFooter(UITableView tableView, nint section)
        {
            //var count = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[section].ToUpper()) select p).Count();
            //return String.Format("Number of Contacts: {0}", count);
            return null;
        }

        public override void CommitEditingStyle(UITableView tableView, UITableViewCellEditingStyle editingStyle, Foundation.NSIndexPath indexPath)
        {
            switch (editingStyle)
            {
                case UITableViewCellEditingStyle.Delete:
                    // remove the item from the underlying data source
                    var pe = (from p in people where p.DisplayName.ToUpper().StartsWith(keys[indexPath.Section]) select p).ToList()[indexPath.Row];
                    people.Remove(pe);
                    // delete the row from the table
                    tableView.DeleteRows(new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
                    // in this example, the person will return when the refresh control fires
                    break;
                case UITableViewCellEditingStyle.None:
                    Console.WriteLine("CommitEditingStyle:None called");
                    break;
            }
        }
        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false; // return false if you wish to disable editing for a specific indexPath or for all rows
        }
        public override string TitleForDeleteConfirmation(UITableView tableView, NSIndexPath indexPath)
        {   // Optional - default text is 'Delete'
            // remove the item from the underlying data source
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

            //var attr = new NSAttributedStringDocumentAttributes();
            //var nsError = new NSError();
            //attr.DocumentType = NSDocumentType.HTML;            
            //cell.TextLabel.AttributedText = new NSAttributedString(string.Format("<b>{0}</b> {1}", pe.FirstName, pe.LastName), attr, ref nsError);            

            cell.DetailTextLabel.Text = pe.Nickname;

            //Console.WriteLine(string.Format("DisplayName: {0}, Nickname: {1}, FirstName: {2}, LastName: {3}", pe.DisplayName, pe.Nickname, pe.FirstName, pe.LastName));

            return cell;
        }
    }
}
