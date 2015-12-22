using System;
using System.Text.RegularExpressions;
using Android.Telephony;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    /// <summary>
    /// Data validation actions
    /// </summary>
    public static class DataValidationUtils
    {
        /// <summary>
        /// Phone number checking
        /// </summary>
        /// <param name="phone">entered phone</param>
        /// <returns>normalized phone or empty</returns>
        public static string IsPhoneValid(string phone)
        {
            try
            {
                return PhoneNumberUtils.NormalizeNumber(phone);
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}