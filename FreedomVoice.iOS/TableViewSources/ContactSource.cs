using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using FreedomVoice.iOS.TableViewCells;
using UIKit;
using Xamarin.Contacts;
using ContactsHelper = FreedomVoice.iOS.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.TableViewSources
{
    public class ContactSource : UITableViewSource
    {
        public List<Contact> ContactsList { private get; set; }

        public string SearchText;

        private bool IsSearchMode => !string.IsNullOrEmpty(SearchText);
#if DEBUG
        public static string[] Keys => new[] { "А", "Б", "В", "Г", "Д", "Е", "Ё", "Ж", "З", "И", "Й", "К", "Л", "М", "Н", "О", "П", "Р", "С", "Т", "У", "Ф", "Х", "Ц", "Ч", "Ш", "Щ", "Ъ", "Ы", "Ь", "Э", "Ю", "Я", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "#" };
#else
        public static string[] Keys => new[]  { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "#" };
#endif
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ContactsList.Count(c => ContactsHelper.ContactSearchPredicate(c, Keys[section]));
        }

        public override string[] SectionIndexTitles(UITableView tableView)
        {
            return IsSearchMode ? null : Keys;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return Keys.Length;
        }

        public override void DraggingStarted(UIScrollView scrollView)
        {
            OnDraggingStarted?.Invoke(null, EventArgs.Empty);
        }

        public override string TitleForHeader(UITableView tableView, nint section)
        {
            return IsSearchMode ? null : RowsInSection(tableView, section) > 0 ? Keys[section] : null;
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

            var person = ContactsList.Where(c => ContactsHelper.ContactSearchPredicate(c, Keys[indexPath.Section])).ToList()[indexPath.Row];
            cell.UpdateCell(person, SearchText);
            cell.LayoutSubviews();

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
        public event EventHandler OnDraggingStarted;
    }
}