using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.TableViewSources;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using FreedomVoice.iOS.ViewControllers.Texts.NewConversation;
using FreedomVoice.iOS.Views;
using FreedomVoice.iOS.Views.Shared;
using UIKit;
using Xamarin.Contacts;
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
    internal struct ContactsViewControllerTexts
    {
        public static readonly string ContactsScreen = "Contacts Screen";
        public static readonly string Search = "Search";
        public static readonly string Contacts = "Contacts";
        public static readonly string NoResult = "No Result";
        public static readonly string NoAvailableContact = "No phone numbers available for this contact.";
        public static readonly string Ok = "Ok";
        public static readonly string SelectNumberFor  = "Select number for ";
        public static readonly string Cancel  = "Cancel";
        public static readonly string Select  = "Select:";
        public static readonly string CallPhone  = "Call Phone";
        public static readonly string SendSMS  = "Send SMS";
        public static readonly string EnDash  = " \u2013 ";
    }

    partial class ContactsViewController : BaseViewController
    {
        protected override string PageName => ContactsViewControllerTexts.ContactsScreen;

        protected List<Contact> _filteredContactList;

        private int FilteredContactsCount => _filteredContactList.Count;

        private ContactSource _contactSource;
        private UISearchBar _contactsSearchBar;
        private UILabel _noResultsLabel;

        private CallerIdView CallerIdView { get; set; }
        private UITableView _contactTableView;

        private bool _hasContactsPermissions;
        private bool _justLoaded;

        protected bool IsSearchMode => _contactsSearchBar?.Text.Length > 0;
        private string SearchText => _contactsSearchBar?.Text;

        protected static MainTabBarController MainTabBarInstance => MainTabBarController.SharedInstance;

        public ContactsViewController()
        {
            _filteredContactList = new List<Contact>();
        }
        
        public ContactsViewController(IntPtr handle) : base(handle)
        {
            _filteredContactList = new List<Contact>();
        }

        public override async void ViewDidLoad()
        {
            _justLoaded = true;
            View.BackgroundColor = UIColor.White;
           
            _hasContactsPermissions = await ContactsHelper.ContactHasAccessPermissionsAsync();
            if (!_hasContactsPermissions)
            {
                View.AddSubview(new NoAccessToContactsView(Theme.ScreenBounds));
                return;
            }

            _contactsSearchBar = new UISearchBar(new CGRect(0, 0, View.Frame.Width, 44))
            {
                Placeholder = ContactsViewControllerTexts.Search,
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

            await ContactsHelper.GetContactsListAsync();

            _contactSource = new ContactSource { ContactsList = ContactsHelper.ContactList };
            _contactSource.OnRowSelected += TableSourceOnRowSelected;
            _contactSource.OnDraggingStarted += TableSourceOnDraggingStarted;

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
                Text = ContactsViewControllerTexts.NoResult,
                Font = UIFont.SystemFontOfSize(28),
                TextColor = Theme.GrayColor,
                TextAlignment = UITextAlignment.Center,
                Center = new CGPoint(View.Center.X, _contactTableView.Center.Y - Theme.TabBarHeight),
                Hidden = true
            };
            View.Add(_noResultsLabel);

            CheckResult(ContactsHelper.ContactsCount);

            base.ViewDidLoad();
        }

        public override void ViewWillAppear(bool animated)
        {
            Title = ContactsViewControllerTexts.Contacts;

            if (_hasContactsPermissions && !_justLoaded)
            {
                _filteredContactList = IsSearchMode ? GetMatchedContacts(SearchText) : ContactsHelper.ContactList;
                _contactSource.ContactsList = _filteredContactList;

                _contactTableView.ReloadData();

                CheckResult(IsSearchMode ? FilteredContactsCount : ContactsHelper.ContactsCount);
            }

            _justLoaded = false;

            SetupNavigationBarButtons();

            PresentationNumber selectedNumber = MainTabBarInstance.GetSelectedPresentationNumber();
            if (selectedNumber != null && _hasContactsPermissions)
                CallerIdView.UpdatePickerData(selectedNumber);

            base.ViewWillAppear(animated);
        }

        public override void ViewDidAppear(bool animated)
        {
            AppDelegate.EnableUserInteraction(UIApplication.SharedApplication);

            base.ViewDidAppear(animated);
        }

        protected virtual void SetupNavigationBarButtons()
        {
            NavigationItem.SetRightBarButtonItem(Appearance.GetLogoutBarButton(this), false);
        }

        private void SearchBarOnSearchButtonClicked(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void TableSourceOnDraggingStarted(object sender, EventArgs e)
        {
            HideKeyboard();
        }

        private void SearchBarOnTextChanged(object sender, UISearchBarTextChangedEventArgs e)
        {
            var search = e.SearchText.Trim();
            _contactSource.SearchText = search;

            _filteredContactList = IsSearchMode ? GetMatchedContacts(search) : ContactsHelper.ContactList;
            _contactSource.ContactsList = _filteredContactList;

            _contactTableView.ReloadData();
            CheckResult(FilteredContactsCount);
        }

        private void SearchBarOnCancelButtonClicked(object sender, EventArgs args)
        {
            HideKeyboard();

            _contactsSearchBar.Text = string.Empty;
            _contactSource.SearchText = string.Empty;
            _contactSource.ContactsList = ContactsHelper.ContactList;
            _contactTableView.ReloadData();

            CheckResult(ContactsHelper.ContactsCount);
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

        protected virtual void TableSourceOnRowSelected(object sender, ContactSource.RowSelectedEventArgs e)
        {
            e.TableView.DeselectRow(e.IndexPath, false);

            var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;

            var person = IsSearchMode ? _filteredContactList.Where(c => ContactsHelper.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row]
                                      : ContactsHelper.ContactList.Where(c => ContactsHelper.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row];

            var phoneNumbers = person.Phones.ToList();

            switch (phoneNumbers.Count)
            {
                case 0:
                    var alertController = UIAlertController.Create(null, ContactsViewControllerTexts.NoAvailableContact, UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create(ContactsViewControllerTexts.Ok, UIAlertActionStyle.Cancel, null));
                    PresentViewController(alertController, true, null);
                    return;
                
                case 1:
                    SelectActionType(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phoneNumbers.First().Number, () => Recents.AddRecent(phoneNumbers.First().Number, person.DisplayName, person.Id));
                    break;
                
                default:
                    var phoneCallController = UIAlertController.Create(ContactsViewControllerTexts.SelectNumberFor + person.DisplayName, null, UIAlertControllerStyle.ActionSheet);
                    foreach (var phone in phoneNumbers)
                    {
                        phoneCallController.AddAction(UIAlertAction.Create(phone.Number + ContactsViewControllerTexts.EnDash + phone.Label, UIAlertActionStyle.Default, async obj =>
                        {
                            SelectActionType(MainTabBarInstance.SelectedAccount.PhoneNumber, selectedCallerId, phone.Number, () => Recents.AddRecent(phone.Number, person.DisplayName, person.Id));
                        }));
                    }
                    phoneCallController.AddAction(UIAlertAction.Create(ContactsViewControllerTexts.Cancel, UIAlertActionStyle.Cancel, null));
                    PresentViewController(phoneCallController, true, null);
                    break;
            }
        }

        private void SelectActionType(string systemNumber, string presentationNumber, string destinationNumberFormatted, Action onCallSuccessAction)
        {
            var alert = new SelectActionDialog();
            alert.Frame = View.Bounds;
            View.AddSubview(alert);

            alert.CallPhoneButtonPressed += () =>
            {
                alert.Hide(alert.RemoveFromSuperview);
                CallPhone(systemNumber, presentationNumber, destinationNumberFormatted, onCallSuccessAction);
            };
            
            alert.SendSMSButtonPressed += () =>
            {
                alert.Hide(alert.RemoveFromSuperview);
                SendSMS(destinationNumberFormatted);
            };
            
            alert.CancelledPressed += () => alert.Hide(alert.RemoveFromSuperview);

            alert.Show();
        }

        private async void CallPhone(string systemNumber, string presentationNumber, string destinationNumberFormatted, Action onSuccessAction)
        {
            await PhoneCall.CreateCallReservation(systemNumber, presentationNumber, destinationNumberFormatted, this, onSuccessAction);
        }

        private void SendSMS(string destinationNumberFormatted)
        {
            var controller = new NewConversationViewController(CallerIdView.SelectedNumber.PhoneNumber, destinationNumberFormatted);
            NavigationController?.PushViewController(controller, true);
        }

        private void HideKeyboard()
        {
            View.EndEditing(true);
        }

        private static List<Contact> GetMatchedContacts(string searchText)
        {
            return ContactsHelper.ContactList.Where(c => ContactsHelper.ContactMatchPredicate(c, searchText)).Distinct().ToList();
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