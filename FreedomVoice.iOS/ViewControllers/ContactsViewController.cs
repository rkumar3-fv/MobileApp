using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;
using CoreGraphics;
using Foundation;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Helpers;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using Xamarin.Contacts;

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

        private bool _havePermissions;

        private CallerIdView CallerIdView { get; set; }

        private static MainTabBarController MainTabBarInstance => MainTabBarController.Instance;

        public ContactsViewController(IntPtr handle) : base(handle)
        {
            _contactList = new List<Contact>();
            _filteredContactList = new List<Contact>();
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            var addressBook = new Xamarin.Contacts.AddressBook();
            _havePermissions = await addressBook.RequestPermission();
            if (!_havePermissions)
            {
                View.AddSubview(new NoAccessToContacts(Theme.ScreenBounds));
                return;
            }

            var frame = new CGRect(View.Frame.Width / 2 - 80 / 2, View.Frame.Height / 2 - 20 / 2, 80, 20);
            _noResultsLabel = new UILabel(frame)
            {
                Text = "No Result",
                MinimumFontSize = 38,
                TextColor = Theme.GrayColor,
                Alpha = 0
            };

            View.Add(_noResultsLabel);

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

            CallerIdView = new CallerIdView(new RectangleF(0, (float)(_contactsSearchBar.Frame.Y + _contactsSearchBar.Frame.Height), (float)View.Frame.Width, 40), MainTabBarInstance.GetPresentationNumbers());

            var headerView = new UIView(new CGRect(0, 0, View.Frame.Width, _contactsSearchBar.Frame.Height + CallerIdView.Frame.Height));
            headerView.AddSubviews(_contactsSearchBar, CallerIdView);

            _contactList = addressBook.ToList();
            _contactSource = new ContactSource { ContactsList = _contactList };
            _contactSource.OnRowSelected += TableSourceOnRowSelected;

            ContactsTableView.Source = _contactSource;
            ContactsTableView.TableHeaderView = headerView;

            ContactsTableView.SectionIndexBackgroundColor = UIColor.Clear;
            ContactsTableView.SectionIndexColor = Theme.BlueColor;

            CheckResult(ContactsCount);
        }

        private void SearchBarOnTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            _filteredContactList = _contactList.Where(c => c.DisplayName.StartsWith(_contactsSearchBar.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            _contactSource.IsSearchMode = !string.IsNullOrEmpty(e.SearchText);
            _contactSource.ContactsList = _filteredContactList;
            ContactsTableView.ReloadData();
            CheckResult(FilteredContactsCount);
        }

        private void SearchBarOnCancelButtonClicked(object sender, EventArgs args)
        {
            _contactsSearchBar.Text = string.Empty;
            _contactsSearchBar.ResignFirstResponder();
            _contactSource.IsSearchMode = false;
            _contactSource.ContactsList = _contactList;
            ContactsTableView.ReloadData();
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

        private void TableSourceOnRowSelected(object sender, ContactSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            if (PhoneCapability.IsAirplaneMode())
            {
                var alertController = UIAlertController.Create(null, "Airplane Mode must be turned off to make calls from the FreedomVoice app.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Settings", UIAlertActionStyle.Default, a => {
                    var url = new NSUrl(UIApplication.OpenSettingsUrlString);
                    UIApplication.SharedApplication.OpenUrl(url);
                }));
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

                PresentViewController(alertController, true, null);
                return;
            }

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;

            var person = _contactList.Where(c => c.DisplayName.StartsWith(_contactSource.Keys[e.IndexPath.Section], StringComparison.OrdinalIgnoreCase)).ToList()[e.IndexPath.Row];
            var phoneNumbers = person.Phones.ToList();

            switch (phoneNumbers.Count)
            {
                case 0:
                    var alertController = UIAlertController.Create(null, "No phone numbers available for this contact.", UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                    PresentViewController(alertController, true, null);
                    return;
                case 1:
                    PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phoneNumbers.First().Number, NavigationController);
                    AddRecent(person.DisplayName, phoneNumbers.First().Number, person.Id);
                    break;
                default:
                    
                    var phoneCallController = UIAlertController.Create("Select number for " + person.DisplayName, null, UIAlertControllerStyle.ActionSheet);
                    foreach (var phone in phoneNumbers)
                    {
                        phoneCallController.AddAction(UIAlertAction.Create(phone.Number + " \u2013 " + phone.Label, UIAlertActionStyle.Default, a => {
                            PhoneCall.CreateCallReservation(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phone.Number, NavigationController);
                            AddRecent(person.DisplayName, phone.Number, person.Id);
                        }));
                    }
                    phoneCallController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                    PresentViewController(phoneCallController, true, null);
                    break;
            }
        }

        private static void AddRecent(string title, string phoneNumber, string contactId)
        {
            MainTabBarInstance.Recents.Add(new Recent(title, phoneNumber, DateTime.Now, contactId));
        }

        private void CheckResult(int contactsCount)
        {
            if (contactsCount == 0)
            {
                _noResultsLabel.Alpha = 1;
                ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noResultsLabel.Alpha = 0;
                ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            MainTabBarInstance.Title = "Contacts";

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null && _havePermissions)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }
    }
}