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

        public CallerIdView CallerIdView { get; private set; }

        public ContactsViewController(IntPtr handle) : base(handle) { }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            const int containerWidth = 80;
            const int containerHeight = 20;

            var frame = new CGRect(View.Frame.Width / 2 - containerWidth / 2, View.Frame.Height / 2 - containerHeight / 2, containerWidth, containerHeight);
            _noResultsLabel = new UILabel(frame)
            {
                Text = "No Result",
                MinimumFontSize = 38f,
                TextColor = UIColor.FromRGB(127, 127, 127),
                Alpha = 0f
            };

            View.Add(_noResultsLabel);

            var addressBook = new Xamarin.Contacts.AddressBook();
            if (!await addressBook.RequestPermission())
            {
                // We don't have the permission to access user's contacts: check if the READ_CONTACTS has been set
                new UIAlertView("Permission denied", "User has denied this app access to their contacts", null, "Close").Show();
                return;
            }

            _contactsSearchBar = new UISearchBar(new CGRect(0, 0, 320, 44))
            {
                Placeholder = "Search",
                AutocapitalizationType = UITextAutocapitalizationType.None,
                SpellCheckingType = UITextSpellCheckingType.No,
                AutocorrectionType = UITextAutocorrectionType.No
            };

            _contactsSearchBar.ShouldBeginEditing += SearchBarOnShouldBeginEditing;
            _contactsSearchBar.ShouldEndEditing += SearchBarOnShouldEndEditing;
            _contactsSearchBar.TextChanged += SearchBarOnTextChanged;
            _contactsSearchBar.CancelButtonClicked += SearchBarOnCancelButtonClicked;

            CallerIdView = new CallerIdView(new RectangleF(0, 44, 320, 40), MainTab?.GetPresentationNumbers());

            var headerView = new UIView(new CGRect(0, 0, 320, 84));
            headerView.AddSubviews(_contactsSearchBar, CallerIdView);

            _contactList = addressBook.ToList();
            _contactSource = new ContactSource { ContactsList = _contactList };
            _contactSource.OnRowSelected += TableSourceOnRowSelected;

            ContactsTableView.Source = _contactSource;
            ContactsTableView.TableHeaderView = headerView;
            ContactsTableView.SectionIndexBackgroundColor = UIColor.Clear;

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

        void SearchBarOnCancelButtonClicked(object sender, EventArgs args)
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
            NavigationController.SetNavigationBarHidden(false, true);
            _contactsSearchBar.ShowsCancelButton = false;
            return true;
        }

        private bool SearchBarOnShouldBeginEditing(UISearchBar searchBar)
        {
            NavigationController.SetNavigationBarHidden(true, true);
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

            if (!PhoneCapability.IsCellularEnabled())
            {
                var alertController = UIAlertController.Create(null, "Your device does not appear to support making cellular voice calls.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
                PresentViewController(alertController, true, null);
                return;
            }

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
                    PhoneCall.CreateCallReservation(string.Empty, string.Empty, string.Empty, phoneNumbers.First().Number);
                    AddRecent(person.DisplayName, phoneNumbers.First().Number);
                    break;
                default:
                    var phoneCallController = UIAlertController.Create("Select number for " + person.DisplayName, null, UIAlertControllerStyle.ActionSheet);
                    foreach (var phone in phoneNumbers)
                    {
                        phoneCallController.AddAction(UIAlertAction.Create(phone.Label + " - " + phone.Number, UIAlertActionStyle.Default, a => {
                            PhoneCall.CreateCallReservation(string.Empty, string.Empty, string.Empty, phone.Number);
                            AddRecent(person.DisplayName, phoneNumbers.First().Number);
                        }));
                    }
                    phoneCallController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
                    PresentViewController(phoneCallController, true, null);
                    break;
            }
        }

        private MainTabBarController MainTab { get { return ParentViewController as MainTabBarController; } }

        private void AddRecent(string title, string phoneNumber)
        {
            var ctrl = ParentViewController as MainTabBarController;
            ctrl?.Recents.Add(new Recent(title, phoneNumber, DateTime.Now));
        }

        void CheckResult(int contactsCount)
        {
            if (contactsCount == 0)
            {
                _noResultsLabel.Alpha = 1f;
                ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            }
            else
            {
                _noResultsLabel.Alpha = 0f;
                ContactsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.SingleLine;
            }
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);

            PresentationNumber selectedNumber = (MainTab as MainTabBarController)?.GetSelectedPresentationNumber();
            if (selectedNumber != null)
                CallerIdView.UpdatePickerData(selectedNumber);
        }
    }
}