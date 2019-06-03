using System;
using FreedomVoice.Core.Utils.Interfaces;
using PhoneNumbers;

namespace FreedomVoice.Core.Utils
{
    public class PhoneFormatter: IPhoneFormatter
    {
        private PhoneNumberUtil Util => PhoneNumberUtil.GetInstance();
        private string DefaultRegion => "US";

        public string Format(string phone)
        {
            var obj = Parse(phone);
            return obj != null ? Util.Format(obj, PhoneNumberFormat.NATIONAL) : "";
        }

        public string Normalize(string phone)
        {
            var obj = Parse(phone);
            return obj == null ? phone : $"{obj.CountryCode}{obj.NationalNumber}{obj.Extension}";
        }

        public string NormalizeNational(string phone)
        {
            var obj = Parse(phone);
            return obj == null ? phone : $"{obj.NationalNumber}{obj.Extension}";
        }

        private PhoneNumber Parse(string phone)
        {
            try
            {
                return Util.Parse(phone, DefaultRegion);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }
        
    }
}
