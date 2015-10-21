using FreedomVoice.Core.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FreedomVoice.iOS.Entities
{
    public class Recent
    {
        public string Title { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DialDate { get; set; }
        public string FormatedDialDate {
            get {
                if (DialDate.Date == DateTime.Now.Date.AddDays(-1))
                    return "Yesterday";

                if (DialDate.Date < DateTime.Now.Date.AddDays(-1))
                    return DialDate.ToString(@"MM/dd/yy", new CultureInfo("en-US"));

                return DialDate.ToString(@"hh:mm tt", new CultureInfo("en-US"));
            }
        }
        public string TitleOrNumber {
            get {
                if (!string.IsNullOrEmpty(Title))
                    return Title;
                return DataFormatUtils.ToPhoneNumber(PhoneNumber);
            }
        }

        public Recent() { }

        public Recent(string title, string phoneNumber, DateTime dialDate) {
            Title = title;
            PhoneNumber = phoneNumber;
            DialDate = dialDate;
        }
    }
}
