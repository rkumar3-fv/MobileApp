﻿using Foundation;
using FreedomVoice.Core.Utils;

namespace FreedomVoice.iOS.Helpers
{
    public static class UserDefault
    {
        const string AccountPhoneNumberKey = "AccountPhoneNumber";

        public static bool IsAuthenticated
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsAuthenticatedUserKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsAuthenticatedUserKey");
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

        public static string AccountPhoneNumber { get; set; }

        public static void SaveAccountPhoneNumber(string userPhoneNumber)
        {
            AccountPhoneNumber = DataFormatUtils.ToPhoneNumber(userPhoneNumber);
            NSUserDefaults.StandardUserDefaults.SetString(AccountPhoneNumber, AccountPhoneNumberKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public static void UpdateFromPreferences()
        {
            AccountPhoneNumber = NSUserDefaults.StandardUserDefaults.StringForKey(AccountPhoneNumberKey);
        }
    }
}