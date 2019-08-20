using System;
using System.Linq;
using System.Text.RegularExpressions;
using FreedomVoice.Core.Utils.Interfaces;
using PhoneNumbers;

namespace FreedomVoice.Core.Utils
{
    public class PhoneFormatter: IPhoneFormatter
    {
        private PhoneNumberUtil Util => PhoneNumberUtil.GetInstance();
        private string DefaultRegion => "US";
        private string DefaultRegionCode => "1";
        private const string Phone3Regex = @"^\(?(\d{3})\)$";
        private const string Phone4Regex = @"^\(?(\d{3})\)?[-. ]?(\d{1,3})$";
        private const string Phone7Regex = @"^\(?(\d{3})\)?[-. ]?(\d{3})[-. ]?(\d{1,3})$";
        private const string Phone10Regex = @"^\(?(\d{3})\)?[-. ]?(\d{3})[-. ]?(\d{2})?(\d{1,2})$";
        private const string Phone11Regex= @"^(\d{1})[-. ]?\(?(\d{3})\)?[-. ]?(\d{3})[-. ]?(\d{2})?(\d{1,2})$";
        
        private const string PlusPhone4Regex = @"^([+]{1})?(\d{1})[-. ]?\(?(\d{1,3})\)$";
        private const string PlusPhone7Regex = @"^([+]{1})?(\d{1})[-. ]?\(?(\d{3})\)?[-. ]?(\d{1,3})$";
        private const string PlusPhone11Regex = @"^([+]{1})?(\d{1})[-. ]?\(?(\d{3})\)?[-. ]?(\d{3})[-. ]?(\d{1,4})$";

        public string Format(string phone)
        {
            if ( string.IsNullOrEmpty(phone) )
                return "";
            var phoneNumber = Regex.Replace(phone, @"\D", "");
            if ( string.IsNullOrEmpty(phone) )
                return phoneNumber;
            if (phoneNumber.Length == 11 && phoneNumber.StartsWith(DefaultRegionCode))
            {
                phoneNumber = phoneNumber.Substring(1);
            }

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
                    format      = "($1) $2 $3$4";
                    break;

                case 10 :
                    phoneParser = new Regex(@"(\d{3})(\d{3})(\d{2})(\d{2})");
                    format      = "($1) $2 $3$4";
                    break;

                case 11 :
                    phoneParser = new Regex(@"(\d{1})(\d{3})(\d{3})(\d{2})(\d{2})");
                    format      = "$1($2) $3 $4$5";
                    break;

                default:
                    phoneParser = new Regex(@"(\d.)");
                    format      = "$1";
                    break;
            }
            var res = phoneParser.Replace( phoneNumber, format );
            if (phone.StartsWith("+"))
            {
                res = res.Insert(0, "+");
            }

            return res;
        }

        public string Reformat(string phone)
        {
            var res = phone;
            if (phone.StartsWith("+"))
            {
                var phone11Regex = new Regex(PlusPhone11Regex);
                var phone7Regex = new Regex(PlusPhone7Regex);
                var phone4Regex = new Regex(PlusPhone4Regex);
                if (phone11Regex.IsMatch(phone))
                    return phone11Regex.Replace(phone, "$1$2 ($3) $4 $5");
                if (phone7Regex.IsMatch(phone))
                    return phone7Regex.Replace(phone, "$1$2 ($3) $4");
                if (phone4Regex.IsMatch(phone))
                    return phone4Regex.Replace(phone, "$1$2 ($3)");
            }
            else
            {
                var phone11Regex = new Regex(Phone11Regex);
                var phone10Regex = new Regex(Phone10Regex);
                var phone7Regex = new Regex(Phone7Regex);
                var phone4Regex = new Regex(Phone4Regex);
                var phone3Regex = new Regex(Phone3Regex);
                if (phone11Regex.IsMatch(phone))
                    res = phone11Regex.Replace(phone, "$1 ($2) $3 $4$5");
                if (phone10Regex.IsMatch(phone))
                    res = phone10Regex.Replace(phone, "($1) $2 $3$4");
                if (phone7Regex.IsMatch(phone))
                    res = phone7Regex.Replace(phone, "($1) $2 $3");
                if (phone4Regex.IsMatch(phone))
                    res = phone4Regex.Replace(phone, "($1) $2");
                if (phone3Regex.IsMatch(phone))
                    res = phone3Regex.Replace(phone, "($1)");
            }

            return res;
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
