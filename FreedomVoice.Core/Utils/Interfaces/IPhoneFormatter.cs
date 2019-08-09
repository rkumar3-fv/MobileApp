using System;
namespace FreedomVoice.Core.Utils.Interfaces
{
    public interface IPhoneFormatter
    {
        /// <summary>
        /// Format the specified phone.
        /// </summary>
        /// <returns>The formatted phone number.</returns>
        /// <param name="phone">Phone.</param>
        string Format(string phone);

        string CustomFormatter(string phone);

        /// <summary>
        /// Remove all formats and pass only digits
        /// </summary>
        /// <returns>The normalize.</returns>
        /// <param name="phone">Phone.</param>
        string Normalize(string phone);

        /// <summary>
        /// Normalize phone without country code
        /// </summary>
        /// <returns>The normalize.</returns>
        /// <param name="phone">Phone.</param>
        string NormalizeNational(string phone);
    }
}
