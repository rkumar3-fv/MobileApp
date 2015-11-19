using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
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
using ContactsHelper = FreedomVoice.iOS.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
    partial class ContactsViewController : UIViewController
    {
        private List<Contact> _contactList;
        private List<Contact> _filteredContactList;

        private int ContactsCount => _contactList.Count;
        private int FilteredContactsCount => _filteredContactList.Count;

        private ContactSource _contactSource;
        private UISearchBar _contactsSearchBar;
        private UILabel _noResultsLabel;

        private CallerIdView CallerIdView { get; set; }
        private UITableView _contactTableView;

        private bool _hasContactsPermissions;
        private bool _justLoaded;

        private bool IsSearchMode => _contactsSearchBar?.Text.Length > 0;
        private string SearchText => _contactsSearchBar?.Text;

        private static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public ContactsViewController(IntPtr handle) : base(handle)
        {
            _contactList = new List<Contact>();
            _filteredContactList = new List<Contact>();
        }

        public override async void ViewDidLoad()
        {
            _justLoaded = true;

            _hasContactsPermissions = await AppDelegate.ContactHasAccessPermissionsAsync();
            if (!_hasContactsPermissions)
            {
                View.AddSubview(new NoAccessToContactsView(Theme.ScreenBounds));
                return;
            }

            _contactsSearchBar = new UISearchBar(new CGRect(0, 0, View.Frame.Width, 44))
            {
                Placeholder = "Search",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocorrectionType = UITextAutocorrectionType.No,
                BarTintColor = Theme.BarBackgroundColor
            };

            var contactsSearchBarCancelButton = UIBarButtonItem.AppearanceWhenContainedIn(typeof(UISearchBar));
            var textAttributes = new UITextAttributes { Font = UIFont.SystemFontOfSize(17, UIFontWeight.Medium), TextColor = Theme.BarButtonColor };
            contactsSearchBarCancelButton.SetTitleTextAttributes(textAttributes, UIControlState.Normal);

            _contactsSearchBar.ShouldBeginEditing += SearchBarOnShouldBeginEditing;
            _contactsSearchBar.ShouldEndEditing += SearchBarOnShouldEndEditing;
            _contactsSearchBar.TextChanged += SearchBarOnTextChanged;
            _contactsSearchBar.CancelButtonClicked += SearchBarOnCancelButtonClicked;
            _contactsSearchBar.SearchButtonClicked += SearchBarOnSearchButtonClicked;

            CallerIdView = new CallerIdView(new RectangleF(0, (float)(_contactsSearchBar.Frame.Y + _contactsSearchBar.Frame.Height), (float)View.Frame.Width, 40), MainTabBarInstance.GetPresentationNumbers());
            var lineView = new LineView(new RectangleF(0, (float)(CallerIdView.Frame.Y + CallerIdView.Frame.Height), (float)Theme.ScreenBounds.Width, 0.5f));

            var headerView = new UIView(new CGRect(0, 0, View.Frame.Width, _contactsSearchBar.Frame.Height + CallerIdView.Frame.Height + lineView.Frame.Height));
            headerView.AddSubviews(_contactsSearchBar, CallerIdView, lineView);
            View.AddSubview(headerView);

            _contactList = await AppDelegate.GetContactsListAsync();
            _contactSource = new ContactSource { ContactsList = _contactList };
            _contactSource.OnRowSelected += TableSourceOnRowSelected;

            var headerHeight = Theme.StatusBarHeight + NavigationController.NavigationBarHeight();
            var insets = new UIEdgeInsets(0, 0, headerHeight, 0);

            var headerViewHeight = headerView.Frame.Height;
            _contactTableView = new UITableView
            {
                Frame = new CGRect(0, headerViewHeight, Theme.ScreenBounds.Width, Theme.ScreenBounds.Height - Theme.TabBarHeight - headerViewHeight),
                Source = _contactSource,
                SectionIndexBackgroundColor = UIColor.Clear,
                SectionIndexColor = Theme.BlueColor,
                ContentInset = insets,
                ScrollIndicatorInsets = insets
            };
            View.Add(_contactTableView);

            var frame = new CGRect(15, 0, Theme.ScreenBounds.Width - 30, 30);
            _noResultsLabel = new UILabel(frame)
            {
                Text = "No Result",
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Center = new CGPoint(View.Center.X, _contactTableView.Center.Y - Theme.TabBarHeight),
                Hidden = true
            };
            View.Add(_noResultsLabel);

            CheckResult(ContactsCount);

            base.ViewDidLoad();
        }

        public override async void ViewWillAppear(bool animated)
        {
            Title = "Contacts";

            if (_hasContactsPermissions && !_justLoaded)
            {
                _contactList = await AppDelegate.GetContactsListAsync();
                _filteredContactList = IsSearchMode ? GetMatchedContacts(SearchText) : _contactList;
                _contactSource.ContactsList = _filteredContactList;

                _contactTableView.ReloadData();

                CheckResult(IsSearchMode ? FilteredContactsCount : ContactsCount);
            }

            _justLoaded = false;

            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null && _hasContactsPermissions)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            View.EndEditing(true);
        }

        private void SearchBarOnSearchButtonClicked(object sender, EventArgs e)
        {
            _contactsSearchBar.ResignFirstResponder();
        }

        private void SearchBarOnTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            _contactSource.SearchText = e.SearchText;

            _filteredContactList = IsSearchMode ? GetMatchedContacts(e.SearchText) : _contactList;
            _contactSource.ContactsList = _filteredContactList;

            _contactTableView.ReloadData();
            CheckResult(FilteredContactsCount);
        }

        private void SearchBarOnCancelButtonClicked(object sender, EventArgs args)
        {
            _contactsSearchBar.Text = string.Empty;
            _contactsSearchBar.ResignFirstResponder();
            _contactSource.SearchText = string.Empty;
            _contactSource.ContactsList = _contactList;
            _contactTableView.ReloadData();
            CheckResult(ContactsCount);
        }

        private bool SearchBarOnShouldEndEditing(UISearchBar searchBar)
        {
            _contactsSearchBar.ShowsCancelButton = false;
            return true;
        }

        private bool SearchBarOnShouldBeginEditing(UISearchBar searchBar)
        {
            _contactsSearchBar.ShowsCancelButton = true;
            return true;
        }

        private async void TableSourceOnRowSelected(object sender, ContactSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;

            var person = IsSearchMode ? _filteredContactList.Where(c => ContactsHelper.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row]
                                      : _contactList.Where(c => ContactsHelper.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row];

            var phoneNumbers = person.Phones.ToList();

            switch (phoneNumbers.Count)
            {
                case 0:
                    var alertController = UIAlertController.Create(null, "No phone numbers available for this contact.", UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                    PresentViewController(alertController, true, null);
                    return;
                case 1:
                    if (await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phoneNumbers.First().Number, NavigationController))
                        AddRecent(person.DisplayName, phoneNumbers.First().Number, person.Id);
                    break;
                default:
                    var phoneCallController = UIAlertController.Create("Select number for " + person.DisplayName, null, UIAlertControllerStyle.ActionSheet);
                    foreach (var phone in phoneNumbers)
                    {
                        phoneCallController.AddAction(UIAlertAction.Create(phone.Number + " \u2013 " + phone.Label, UIAlertActionStyle.Default, async a => {
                            if (await PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phone.Number, NavigationController))
                                AddRecent(person.DisplayName, phone.Number, person.Id);
                        }));
                    }
                    phoneCallController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                    PresentViewController(phoneCallController, true, null);
                    break;
            }
        }

        private List<Contact> GetMatchedContacts(string searchText)
        {
            return _contactList.Where(c => ContactsHelper.ContactMatchPredicate(c, searchText)).ToList();
        }

        private static void AddRecent(string title, string phoneNumber, string contactId)
        {
            MainTabBarInstance.Recents.Add(new Recent(title, phoneNumber, DateTime.Now, contactId));
        }

        private void CheckResult(int contactsCount)
        {
            if (contactsCount == 0)
            {
                _noResultsLabel.Hidden = false;
                _contactTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noResultsLabel.Hidden = true;
                _contactTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }
    }
}