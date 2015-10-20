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
        public bool IsSearchMode;

        private string[] _keys;
        public string[] Keys
        {
            get { return _keys ?? (_keys = Letters.Where(l => ContactsList.Any(p => p.DisplayName.StartsWith(l, StringComparison.OrdinalIgnoreCase))).ToArray()); }
        }

        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ContactsList.Count(c => c.DisplayName.StartsWith(Keys[section], StringComparison.OrdinalIgnoreCase));
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

            var person = ContactsList.Where(c => c.DisplayName.StartsWith(Keys[indexPath.Section], StringComparison.OrdinalIgnoreCase)).ToList()[indexPath.Row];
            cell.TextLabel.Text = person.DisplayName;
            cell.DetailTextLabel.Text = person.Nickname;

            return cell;
        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            OnRowSelected?.Invoke(this, new RowSelectedEventArgs(tableView, indexPath));
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