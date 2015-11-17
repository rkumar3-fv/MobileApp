using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using FreedomVoice.iOS.TableViewCells;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.TableViewSources
{
    public class ContactSource : UITableViewSource
    {
        private static readonly string[] Letters = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        private List<Contact> _contactsList;
        public List<Contact> ContactsList
        {
            private get { return _contactsList; }
            set
            {
                _keys = null;
                _contactsList = value;
            }
        }

        public string SearchText;

        private bool IsSearchMode => !string.IsNullOrEmpty(SearchText);

        private string[] _keys;
        public string[] Keys
        {
            get { return _keys ?? (_keys = Letters.Where(l => ContactsList.Any(c => ContactSearchPredicate(c, l))).ToArray()); }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ContactsList.Count(c => ContactSearchPredicate(c, Keys[section]));
        }

        public override string[] SectionIndexTitles(UITableView tableView)
        {
            return IsSearchMode ? null : Keys;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return Keys.Length;
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return IsSearchMode ? null : Keys[section];
        }

        public override string TitleForFooter(UITableView tableView, nint section)
        {
            return null;
        }

        public override bool CanEditRow(UITableView tableView, NSIndexPath indexPath)
        {
            return false;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = tableView.DequeueReusableCell(ContactCell.ContactCellId) as ContactCell ?? new ContactCell();

            var person = ContactsList.Where(c => ContactSearchPredicate(c, Keys[indexPath.Section])).ToList()[indexPath.Row];
            cell.UpdateCell(person, SearchText);
            cell.LayoutSubviews();

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            OnRowSelected?.Invoke(this, new RowSelectedEventArgs(tableView, indexPath));
        }

        private static bool ContactSearchPredicate(Contact c, string firstLetter)
        {
            return c.LastName?.StartsWith(firstLetter, StringComparison.OrdinalIgnoreCase) ?? c.DisplayName != null && c.DisplayName.StartsWith(firstLetter, StringComparison.OrdinalIgnoreCase);
        } 

        public class RowSelectedEventArgs : EventArgs
        {
            public UITableView TableView { get; private set; }
            public NSIndexPath IndexPath { get; private set; }

            public RowSelectedEventArgs(UITableView tableView, NSIndexPath indexPath)
            {
                TableView = tableView;
                IndexPath = indexPath;
            }
        }

        public event EventHandler<RowSelectedEventArgs> OnRowSelected;
    }
}