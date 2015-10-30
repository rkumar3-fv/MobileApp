using Foundation;
using FreedomVoice.Core.Utils;
using System;

namespace FreedomVoice.iOS.Helpers
{
    public class Settings
    {
        const string accountPhoneNumberKey = "AccountPhoneNumber";                    
            
        public static string AccountPhoneNumber { get; set; }

        public static void SaveAccountPhoneNumber(string userPhoneNumber)
        {
            AccountPhoneNumber = DataFormatUtils.ToPhoneNumber(userPhoneNumber);
            NSUserDefaults.StandardUserDefaults.SetString(AccountPhoneNumber, accountPhoneNumberKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        public static void UpdateFromPreferences()
        {            
            AccountPhoneNumber = NSUserDefaults.StandardUserDefaults.StringForKey(accountPhoneNumberKey);
        }
    }
}
