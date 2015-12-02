using Foundation;
using Security;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class KeyChain
    {
        private const string ServiceId = "FreedomVoice";

        /// <summary>
        /// Gets a username strored in keychain.
        /// </summary>
        /// <returns>The password or NULL if no matching record was found.</returns>
        public static string GetUsername()
        {
            var existingRecord = new SecRecord(SecKind.GenericPassword)
            {
                Service = ServiceId,
                Label = "Password"
            };

            SecStatusCode resultCode;
            var data = SecKeyChain.QueryAsRecord(existingRecord, out resultCode);

            return resultCode == SecStatusCode.Success ? NSString.FromData(data.Account, NSStringEncoding.UTF8) : null;
        }

        /// <summary>
        /// Sets a password for a specific username.
        /// </summary>
        /// <param name="username">the username to add the password for. Not case sensitive.  May not be NULL.</param>
        /// <param name="password">the password to associate with the record. May not be NULL.</param>
        /// <returns>SecStatusCode.Success if everything went fine, otherwise some other status</returns>
        public static void SetPasswordForUsername(string username, string password)
        {
            var existingRecord = new SecRecord(SecKind.GenericPassword)
            {
                Account = username,
                Label = "Password",
                Service = ServiceId
            };

            SecStatusCode resultCode;
            var data = SecKeyChain.QueryAsRecord(existingRecord, out resultCode);

            if (resultCode == SecStatusCode.Success)
            {
                resultCode = SecKeyChain.Remove(existingRecord);

                if (resultCode == SecStatusCode.Success)
                {
                    SecKeyChain.Add(new SecRecord(SecKind.GenericPassword)
                    {
                        Label = "Password",
                        Account = username,
                        Service = ServiceId,
                        ValueData = NSData.FromString(password, NSStringEncoding.UTF8)
                    });
                }
            }
            else
            if (resultCode == SecStatusCode.ItemNotFound)
            {
                SecKeyChain.Add(new SecRecord(SecKind.GenericPassword)
                {
                    Label = "Password",
                    Account = username,
                    Service = ServiceId,
                    ValueData = NSData.FromString(password, NSStringEncoding.UTF8)
                });
            }
        }

        /// <summary>
        /// Gets a password for a specific username.
        /// </summary>
        /// <param name="username">the username to query. Not case sensitive. May not be NULL.</param>
        /// <returns>The password or NULL if no matching record was found.</returns>
        public static string GetPasswordForUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;

            var existingRecord = new SecRecord(SecKind.GenericPassword)
            {
                Account = username,
                Label = "Password",
                Service = ServiceId
            };

            SecStatusCode resultCode;
            var data = SecKeyChain.QueryAsRecord(existingRecord, out resultCode);

            return resultCode == SecStatusCode.Success ? NSString.FromData(data.ValueData, NSStringEncoding.UTF8) : null;
        }

        /// <summary>
		/// Deletes a username/password record.
		/// </summary>
		/// <param name="userName">the username to query. Not case sensitive. May not be NULL.</param>
		/// <returns>Status code</returns>
		public static void DeletePasswordForUsername(string userName)
        {
            var queryRec = new SecRecord(SecKind.GenericPassword) { Service = ServiceId, Label = "Password", Account = userName };
            var code = SecKeyChain.Remove(queryRec);
        }
    }
}