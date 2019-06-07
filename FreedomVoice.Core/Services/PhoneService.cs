using System.Text.RegularExpressions;

namespace FreedomVoice.Core.Services
{
    public class PhoneService
    {
        private const string US_CODE = "1";
        private const string PAID_CALL_NUMBERS = "8800";
        private const int COUNT_DIGITS_IN_LOCAL_NUMBER = 10;

        public static string GetClearPhone(string phone)
        {
            string result = Regex.Replace(phone, @"[^\d]", "");
            if (result.Length == COUNT_DIGITS_IN_LOCAL_NUMBER)
                result = $"{US_CODE}{result}";
            return result;
        }

        public static string GetReadytoSendPhone(string phone)
        {
            string result = Regex.Replace(phone, @"[^\d]", "");
            if (result.Length > COUNT_DIGITS_IN_LOCAL_NUMBER && !result.StartsWith(PAID_CALL_NUMBERS))
                result = $"+{result}";
            return result;
        }
    }
}