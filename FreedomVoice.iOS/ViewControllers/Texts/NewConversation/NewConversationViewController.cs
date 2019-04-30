using System;
using System.Text;
using System.Text.RegularExpressions;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.Entities;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewControllers.Texts.NewConversation
{
	internal struct NewConversationTexts
	{
		public const string NewMessage = "New Message";
		public const string Error = "Error";
		public const string PhoneNumberIsEmpty = "Phone number is empty.";
		public const string OK = "Ok";
		public const string To = "To:";
		public const string Plus = "+";
	}
	
	public class NewConversationViewController: ConversationViewController
	{

		private readonly AddContactView _addContactView = new AddContactView();
		private readonly IContactNameProvider _contactNameProvider;

		public NewConversationViewController()
		{
			_contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			Title = NewConversationTexts.NewMessage;
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			_contactNameProvider.ContactsUpdated += ProviderOnContactsUpdated;
			_addContactView.AddContactButtonPressed += AddContactButtonPressed;
			_addContactView.PhoneNumberChanged += PhoneNumberChanged;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			View.EndEditing(true);
			_addContactView.AddContactButtonPressed -= AddContactButtonPressed;
			_addContactView.PhoneNumberChanged -= PhoneNumberChanged;
			_contactNameProvider.ContactsUpdated -= ProviderOnContactsUpdated;
		}

		public override void ViewDidAppear(bool animated)
		{
			base.ViewDidAppear(animated);
			_addContactView.BecomeFirstResponder();
		}

		protected override void _SetupViews()
		{
			base._SetupViews();
			View.AddSubview(_addContactView);
			_callerIdView.Alpha = 0;
		}

		protected override void _SetupConstraints()
		{
			base._SetupConstraints();

			_addContactView.TranslatesAutoresizingMaskIntoConstraints = false;
			_addContactView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
			_addContactView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
			_addContactView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
		}

		protected override void _SetupData()
		{
			View.AddGestureRecognizer(new UITapGestureRecognizer((obj) => View.EndEditing(true)));
		}

		protected override void SendButtonPressed()
		{
			if (string.IsNullOrWhiteSpace(_addContactView.Text))
			{
				var alert = UIAlertController.Create(NewConversationTexts.Error, NewConversationTexts.PhoneNumberIsEmpty, UIAlertControllerStyle.Alert);
				var action = UIAlertAction.Create(NewConversationTexts.OK, UIAlertActionStyle.Default, null);
				alert.AddAction(action);
				PresentViewController(alert, true, null);
				return;
			}
			
			base.SendButtonPressed();
			UIView.Animate(0.24, () =>
			{
				_callerIdView.Alpha = 1;
				_addContactView.Alpha = 0;
			});

			CurrentPhone = new PresentationNumber(_addContactView.Text);
			UpdateTitle(_contactNameProvider.GetFormattedPhoneNumber(_addContactView.Text));

			//TODO SEND MESSAGE LOGIC HERE
			base._SetupData();
		}
		
		private void AddContactButtonPressed()
		{
			var controller = new ContactsPickerViewController();
			controller.PhoneNumberSelected += PhoneNumberSelected;
			controller.Cancelled += Cancelled;
			NavigationController.PushViewController(controller, true);
		}

		private void Cancelled(ContactsPickerViewController contactsPickerViewController)
		{
			contactsPickerViewController.PhoneNumberSelected -= PhoneNumberSelected;
			contactsPickerViewController.Cancelled -= Cancelled;
			NavigationController.PopToViewController(this, true);
		}

		private void PhoneNumberSelected(ContactsPickerViewController contactsPickerViewController, Contact contact, Phone phone)
		{
			contactsPickerViewController.PhoneNumberSelected -= PhoneNumberSelected;
			contactsPickerViewController.Cancelled -= Cancelled;
			NavigationController.PopToViewController(this, true);

			_addContactView.Text = phone.Number;
			UpdateTitle(phone.Number, contact.DisplayName);
		}
		
		private void ProviderOnContactsUpdated(object sender, EventArgs e)
		{
			PhoneNumberChanged(_addContactView.Text);
		}
		
		private void PhoneNumberChanged(string phone)
		{
			if (string.IsNullOrWhiteSpace(phone))
			{
				UpdateTitle(phone);
				return;
			}

			var clearPhone = _contactNameProvider.GetClearPhoneNumber(phone);
			UpdateTitle(clearPhone);
		}

		private void UpdateTitle(string phone, string name = null)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				Title = name;
				return;
			}
			
			var phoneOwner = _contactNameProvider.GetNameOrNull(phone);
			if (!string.IsNullOrWhiteSpace(phoneOwner))
			{
				Title = phoneOwner;
				return;
			}

			Title = NewConversationTexts.NewMessage;
		}
	}
}