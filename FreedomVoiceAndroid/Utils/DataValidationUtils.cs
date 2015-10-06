using System;
using System.Net.Mail;

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
            try
            {
                var mailAddress = new MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
    }
}