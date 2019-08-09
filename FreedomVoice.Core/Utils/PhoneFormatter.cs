using System;
using System.Text.RegularExpressions;
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
            return obj != null ? Util.Format(obj, PhoneNumberFormat.NATIONAL) : phone;
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

        public string CustomFormatter(string phone)
        {
            string Phone3Regex = @"^\(?([0-9]{3})\)$";
            string Phone4Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{1,3})$";
            string Phone7Regex = @"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{1,4})$";
            string Phone11Regex = @"^([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$";

            string PlusPhone4Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{1,3})\)$";
            string PlusPhone7Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{1,3})$";
            string PlusPhone11Regex = @"^([+]{1})?([0-9]{1})[-. ]?\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{1,4})$";

            if (phone.StartsWith("+"))
            {
                var phone11Regex = new Regex(PlusPhone11Regex);
                var phone7Regex = new Regex(PlusPhone7Regex);
                var phone4Regex = new Regex(PlusPhone4Regex);
                if (phone11Regex.IsMatch(phone))
                    return phone11Regex.Replace(phone, "$1$2 ($3) $4-$5");
                if (phone7Regex.IsMatch(phone))
                    return phone7Regex.Replace(phone, "$1$2 ($3) $4");
                if (phone4Regex.IsMatch(phone))
                    return phone4Regex.Replace(phone, "$1$2 ($3)");
            }
            else
            {
                var phone11Regex = new Regex(Phone11Regex);
                var phone7Regex = new Regex(Phone7Regex);
                var phone4Regex = new Regex(Phone4Regex);
                var phone3Regex = new Regex(Phone3Regex);
                if (phone11Regex.IsMatch(phone))
                    return phone11Regex.Replace(phone, "$1 ($2) $3-$4");
                if (phone7Regex.IsMatch(phone))
                    return phone7Regex.Replace(phone, "($1) $2-$3");
                if (phone4Regex.IsMatch(phone))
                    return phone4Regex.Replace(phone, "($1) $2");
                if (phone3Regex.IsMatch(phone))
                    return phone3Regex.Replace(phone, "($1)");
            }
            return phone;
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
