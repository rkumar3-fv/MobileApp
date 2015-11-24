using Foundation;
using FreedomVoice.Core.Utils;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class UserDefault
    {
        public static bool IsAuthenticated
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsAuthenticatedUserKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsAuthenticatedUserKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static int PoolingInterval
        {
            get { return (int)NSUserDefaults.StandardUserDefaults.IntForKey("PoolingIntervalKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetInt(value, "PoolingIntervalKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static string LastUsedAccount
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("LastUsedAccountKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetString(value, "LastUsedAccountKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static bool IsLaunchedBefore
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsLaunchedBeforeKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsLaunchedBeforeKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static string RequestCookie
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("RequestCookieKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetString(value, "RequestCookieKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static string AccountPhoneNumber { get; private set; }

        public static void SaveAccountPhoneNumber(string userPhoneNumber)
        {
            AccountPhoneNumber = DataFormatUtils.ToPhoneNumber(userPhoneNumber);
            NSUserDefaults.StandardUserDefaults.SetString(AccountPhoneNumber, "AccountPhoneNumberKey");
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public static void UpdateFromPreferences()
        {
            var phoneNumber = NSUserDefaults.StandardUserDefaults.StringForKey("AccountPhoneNumberKey");
            if (!string.IsNullOrEmpty(phoneNumber))
                AccountPhoneNumber = phoneNumber;
        }
    }
}