using System;
using Foundation;
using Security;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class KeyChain
    {
        private const string ServiceId = "FreedomVoice";

        /// <summary>
        /// Sets a password for a specific username.
        /// </summary>
        /// <param name="userName">the username to add the password for. Not case sensitive.  May not be NULL.</param>
        /// <param name="password">the password to associate with the record. May not be NULL.</param>
        /// <returns>SecStatusCode.Success if everything went fine, otherwise some other status</returns>
        public static SecStatusCode SetPasswordForUsername(string userName, string password)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            if (string.IsNullOrEmpty(password))
                throw new ArgumentNullException(nameof(password));

            userName = userName.ToLower();

            DeletePasswordForUsername(userName);

            return SecKeyChain.Add(new SecRecord(SecKind.GenericPassword)
            {
                Service = ServiceId,
                Label = "Password",
                Account = userName,
                Generic = NSData.FromString(password, NSStringEncoding.UTF8),
                Accessible = SecAccessible.WhenUnlockedThisDeviceOnly
            });
        }

        /// <summary>
        /// Gets a username strored in keychain.
        /// </summary>
        /// <returns>The password or NULL if no matching record was found.</returns>
        public static string GetUsername()
        {
            SecStatusCode code;

            var queryRec = new SecRecord(SecKind.GenericPassword) { Service = ServiceId, Label = "Password" };
            queryRec = SecKeyChain.QueryAsRecord(queryRec, out code);

            // If found, try to get username.
            if (code == SecStatusCode.Success && queryRec?.Generic != null)
                return NSString.FromData(queryRec.Account, NSStringEncoding.UTF8);

            // Something went wrong.
            return null;
        }

        /// <summary>
        /// Gets a password for a specific username.
        /// </summary>
        /// <param name="userName">the username to query. Not case sensitive. May not be NULL.</param>
        /// <returns>The password or NULL if no matching record was found.</returns>
        public static string GetPasswordForUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            userName = userName.ToLower();

            SecStatusCode code;

            var queryRec = new SecRecord(SecKind.GenericPassword) { Service = ServiceId, Label = "Password", Account = userName };
            queryRec = SecKeyChain.QueryAsRecord(queryRec, out code);

            // If found, try to get password.
            if (code == SecStatusCode.Success && queryRec?.Generic != null)
                return NSString.FromData(queryRec.Generic, NSStringEncoding.UTF8);

            // Something went wrong.
            return null;
        }

        /// <summary>
		/// Deletes a username/password record.
		/// </summary>
		/// <param name="userName">the username to query. Not case sensitive. May not be NULL.</param>
		/// <returns>Status code</returns>
		public static SecStatusCode DeletePasswordForUsername(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                throw new ArgumentNullException(nameof(userName));

            userName = userName.ToLower();

            var queryRec = new SecRecord(SecKind.GenericPassword) { Service = ServiceId, Label = "Password", Account = userName };
            return SecKeyChain.Remove(queryRec);
        }
    }
}
