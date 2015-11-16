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
        /// E-Mail format checking
        /// </summary>
        /// <param name="email">entered e-mail</param>
        /// <returns>validation result</returns>
        public static bool IsEmailValid(string email)
        {
            return Regex.IsMatch(email, @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$");
        }

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