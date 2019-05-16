using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FreedomVoice.Core.ViewModels;

namespace FreedomVoice.iOS.Core.Utilities.Helpers
{
	public class ContactNameProvider: IContactNameProvider
	{

		private Dictionary<string, string> _contactNames = new Dictionary<string, string>();

		public event EventHandler ContactsUpdated;

		public ContactNameProvider()
		{
			ContactItemsDidReceive(null, null);
			Contacts.ItemsChanged += ContactItemsDidReceive;
		}

		public void RequestContacts()
		{
			Contacts.GetContactsListAsync();
		}

		public string GetName(string phone)
		{
			return _contactNames.ContainsKey(phone) ? _contactNames[phone] : FormatPhoneNumber(phone);
		}

		public string GetNameOrNull(string phone)
		{
			return _contactNames.ContainsKey(phone) ? _contactNames[phone] : null;
		}

		public string GetFormattedPhoneNumber(string phoneNumber)
		{
			return FormatPhoneNumber(phoneNumber);
		}

		public string GetClearPhoneNumber(string formattedNumber)
		{
			if (string.IsNullOrWhiteSpace(formattedNumber))
				return formattedNumber;

			const string pattern = @"\d"; 

			var sb = new StringBuilder(); 
			foreach (Match m in Regex.Matches(formattedNumber, pattern)) 
				sb.Append(m); 

			return sb.ToString();
		}

		private void ContactItemsDidReceive(object sender, EventArgs e)
		{
			var debugString = new StringBuilder();
			foreach(var contact in Contacts.ContactList)
			{
				foreach(var phone in contact.Phones) {
					var raw = Regex.Replace(phone.Number, @"\D", "");
					_contactNames[raw] = contact.DisplayName;
					debugString.AppendLine($"{raw}: {contact.DisplayName}");
				}

			}
			
			Console.WriteLine($"[{GetType()}] Contact items received: {debugString} ");
			
			Contacts.ItemsChanged -= ContactItemsDidReceive;
			ContactsUpdated?.Invoke(this, null);

		}
        
		private string FormatPhoneNumber(string phoneNumber) {

			if ( String.IsNullOrEmpty(phoneNumber) )
				return phoneNumber;

			Regex phoneParser;
			string format;

			switch( phoneNumber.Length ) {

				case 5 :
					phoneParser = new Regex(@"(\d{3})(\d{2})");
					format      = "$1 $2";
					break;

				case 6 :
					phoneParser = new Regex(@"(\d{2})(\d{2})(\d{2})");
					format      = "$1 $2 $3";
					break;

				case 7 :
					phoneParser = new Regex(@"(\d{3})(\d{2})(\d{2})");
					format      = "$1 $2 $3";
					break;

				case 8 :
					phoneParser = new Regex(@"(\d{4})(\d{2})(\d{2})");
					format      = "$1 $2 $3";
					break;

				case 9 :
					phoneParser = new Regex(@"(\d{4})(\d{3})(\d{2})(\d{2})");
					format      = "($1 $2 $3 $4";
					break;

				case 10 :
					phoneParser = new Regex(@"(\d{3})(\d{3})(\d{2})(\d{2})");
					format      = "($1) $2-$3$4";
					break;

				case 11 :
					phoneParser = new Regex(@"(\d{4})(\d{3})(\d{2})(\d{2})");
					format      = "$1 $2 $3 $4";
					break;

				default:
					return phoneNumber;

			}

			return phoneParser.Replace( phoneNumber, format );

		}
	}
}