using FreedomVoice.Core.Utils;
using System;
using System.Globalization;

namespace FreedomVoice.iOS.Entities
{
    public class Recent : ICloneable
    {
        public string Title { get; set; }
        public string PhoneNumber { get; }
        public DateTime DialDate { get; set; }
        public string ContactId { get; set; }

        public string FormatedDialDate => DialDate.Date == DateTime.Now.Date.AddDays(-1) ? "Yesterday" : DialDate.ToString(DialDate.Date < DateTime.Now.Date.AddDays(-1) ? @"MM/dd/yy" : @"hh:mm tt", new CultureInfo("en-US"));

        public string TitleOrNumber => !string.IsNullOrEmpty(Title) ? Title : DataFormatUtils.ToPhoneNumber(PhoneNumber);

        public Recent(string title, string phoneNumber, DateTime dialDate, string contactId = "")
        {
            Title = title;
            PhoneNumber = phoneNumber;
            DialDate = dialDate;
            ContactId = contactId;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}