using System;
using FreedomVoice.iOS.Entities;
using UIKit;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewControllers.Texts.NewConversation
{
	public class NewConversationViewController: ConversationViewController
	{

		private readonly AddContactView _addContactView = new AddContactView();

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			Title = "New Message";
		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
			_addContactView.AddContactButtonPressed += AddContactButtonPressed;
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);
			View.EndEditing(true);
			_addContactView.AddContactButtonPressed -= AddContactButtonPressed;
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
				var alert = UIAlertController.Create("Error", "Phone number is empty.", UIAlertControllerStyle.Alert);
				var action = UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null);
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
			Title = CurrentPhone.FormattedPhoneNumber;

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

		private void PhoneNumberSelected(ContactsPickerViewController contactsPickerViewController, Phone phone)
		{
			contactsPickerViewController.PhoneNumberSelected -= PhoneNumberSelected;
			contactsPickerViewController.Cancelled -= Cancelled;
			NavigationController.PopToViewController(this, true);

			_addContactView.Text = phone.Number;
		}
	}
}