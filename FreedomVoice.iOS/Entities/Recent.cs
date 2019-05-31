using System;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;

namespace FreedomVoice.iOS.Entities
{
    public class Recent
    {
        public string Title { get; set; }
        public string PhoneNumber { get; }
        public DateTime DialDate { get; set; }
        public string ContactId { get; set; }
        public int CallsQuantity { get; set; }

        public string FormatedDialDate => DataFormatUtils.ToShortFormattedDate("Yesterday", DialDate);

        public string TitleOrNumber => string.Concat(!string.IsNullOrEmpty(Title) ? Title : ServiceContainer.Resolve<IPhoneFormatter>().Format(PhoneNumber), CallsQuantity > 1 ? " (" + CallsQuantity + ")" : "");

        public Recent(string title, string phoneNumber, DateTime dialDate, string contactId = "")
        {
            Title = title;
            PhoneNumber = phoneNumber;
            DialDate = dialDate;
            ContactId = contactId;
            CallsQuantity = 1;
        }
    }
}