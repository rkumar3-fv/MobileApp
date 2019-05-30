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
            var number = Util.Parse(phone, DefaultRegion);
            return Util.Format(number, PhoneNumberFormat.NATIONAL);
        }

        public string Parse(string phone)
        {
            return Util.Parse(phone, DefaultRegion).ToString();
        }
    }
}
