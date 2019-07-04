using System;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Utilities;
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
		private readonly string _preselectedToPhone;

		private readonly AddContactView _addContactView = new AddContactView();

		public NewConversationViewController()
		{
			_preselectedToPhone = null;
		}
		
		public NewConversationViewController(string currentNumber, string toPhone)
		{
			CurrentPhone = new PresentationNumber(currentNumber);
			_preselectedToPhone = toPhone;
		}

		public override void ViewDidLoad()
		{
			Title = NewConversationTexts.NewMessage;
			base.ViewDidLoad();
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			Presenter.ContactsUpdated += ProviderOnContactsUpdated;
			Presenter.MessageSent += PresenterOnMessageSent;
			_addContactView.AddContactButtonPressed += AddContactButtonPressed;
			_addContactView.PhoneNumberChanged += PhoneNumberChanged;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			View.EndEditing(true);
			_addContactView.AddContactButtonPressed -= AddContactButtonPressed;
			_addContactView.PhoneNumberChanged -= PhoneNumberChanged;
			Presenter.ContactsUpdated -= ProviderOnContactsUpdated;
			Presenter.MessageSent -= PresenterOnMessageSent;
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
			base._SetupData();
			_addContactView.Text = _preselectedToPhone;
			UpdateTitle(_preselectedToPhone);
			CheckCurrentConversation();
		}

		protected override async void SendButtonPressed()
		{
			if (string.IsNullOrWhiteSpace(_addContactView.Text))
			{
				var alert = UIAlertController.Create(NewConversationTexts.Error,
					NewConversationTexts.PhoneNumberIsEmpty, UIAlertControllerStyle.Alert);
				var action = UIAlertAction.Create(NewConversationTexts.OK, UIAlertActionStyle.Default, null);
				alert.AddAction(action);
				PresentViewController(alert, true, null);
				return;
			}

			if (string.IsNullOrWhiteSpace(_chatField.Text))
				return;

			if (Presenter.ConversationId.HasValue)
			{
				base.SendButtonPressed();
				return;
			}

			if (string.IsNullOrWhiteSpace(CurrentPhone.PhoneNumber) || string.IsNullOrWhiteSpace(_addContactView.Text))
				return;
                
			_chatField.SetSending(true);
			
			var conversationId = await Presenter.SendMessage(CurrentPhone.PhoneNumber, _addContactView.Text, _chatField.Text);
			ConversationId = Presenter.ConversationId = conversationId;

			_chatField.SetSending(false);

			if (conversationId.HasValue)
			{
                _chatField.Text = "";
                UIView.Animate(0.24, () =>
				{
					_callerIdView.Alpha = 1;
					_addContactView.Alpha = 0;
				});
				
				UpdateTitle(_addContactView.Text, phonePlaceholder: Presenter.GetFormattedPhoneNumber(_addContactView.Text));
			}
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
			CheckCurrentConversation();
			
			if (string.IsNullOrWhiteSpace(phone))
			{
				UpdateTitle(phone);
				return;
			}

			var clearPhone = Presenter.GetClearPhoneNumber(phone);
			UpdateTitle(clearPhone);
		}
		
		private void PresenterOnMessageSent(object sender, EventArgs e)
		{
			
		}

		private void UpdateTitle(string phone, string name = null, string phonePlaceholder = NewConversationTexts.NewMessage)
		{
			if (!string.IsNullOrWhiteSpace(name))
			{
				Title = name;
				return;
			}

			if (string.IsNullOrWhiteSpace(phone))
			{
				Title = phonePlaceholder;
				return;
			}
			
			var phoneOwner = Presenter.GetNameOrNull(phone);
			if (!string.IsNullOrWhiteSpace(phoneOwner))
			{
				Title = phoneOwner;
				return;
			}

			Title = phonePlaceholder;
		}

		private async void CheckCurrentConversation()
		{
			if (string.IsNullOrWhiteSpace(CurrentPhone.PhoneNumber) || string.IsNullOrWhiteSpace(_addContactView.Text))
				return;
			
			var conversationId = await Presenter.GetConversationId(CurrentPhone.PhoneNumber, _addContactView.Text);

			if (conversationId.HasValue)
			{
				Console.WriteLine($"Load history {conversationId}");
				Presenter.ConversationId = ConversationId = conversationId;
				Presenter.ReloadAsync();
			}
			else
			{
				Console.WriteLine("Clear history");
				Presenter.ConversationId = ConversationId = null;
				Presenter.Clear();
			}
		}
	}
}