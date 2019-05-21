using System;
using System.Linq;
using FreedomVoice.iOS.TableViewSources;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewControllers
{
	partial class ContactsPickerViewController : ContactsViewController
	{

		public Action<ContactsPickerViewController, Contact, Phone> PhoneNumberSelected;
		public Action<ContactsPickerViewController> Cancelled;

		protected override void SetupNavigationBarButtons()
		{
			NavigationItem.SetHidesBackButton(true, false);
			NavigationItem.SetRightBarButtonItem(new UIBarButtonItem(UIBarButtonSystemItem.Cancel, (s, e) =>
			{
				Cancelled?.Invoke(this);
			}), true);
		}

		protected override void TableSourceOnRowSelected(object sender, ContactSource.RowSelectedEventArgs e)
		{
			e.TableView.DeselectRow(e.IndexPath, false);

			var selectedCallerId = MainTabBarInstance.GetSelectedPresentationNumber().PhoneNumber;

			var person = IsSearchMode ? _filteredContactList.Where(c => Utilities.Helpers.Contacts.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row]
				: Utilities.Helpers.Contacts.ContactList.Where(c => Utilities.Helpers.Contacts.ContactSearchPredicate(c, ContactSource.Keys[e.IndexPath.Section])).ToList()[e.IndexPath.Row];

			var phoneNumbers = person.Phones.ToList();

			switch (phoneNumbers.Count)
			{
				case 0:
					var alertController = UIAlertController.Create(null, "No phone numbers available for this contact.", UIAlertControllerStyle.Alert);
					alertController.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Cancel, null));
					PresentViewController(alertController, true, null);
					return;
				case 1:
					PhoneNumberSelected?.Invoke(this,  person, phoneNumbers.FirstOrDefault());
					break;
				default:
					var phoneCallController = UIAlertController.Create("Select number for " + person.DisplayName, null, UIAlertControllerStyle.ActionSheet);
					foreach (var phone in phoneNumbers)
					{
						phoneCallController.AddAction(UIAlertAction.Create(phone.Number + " \u2013 " + phone.Label, UIAlertActionStyle.Default,
							obj => {
								PhoneNumberSelected?.Invoke(this, person, phone);
							}));
					}
					phoneCallController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));
					PresentViewController(phoneCallController, true, null);
					break;
			}
		}

	}
}