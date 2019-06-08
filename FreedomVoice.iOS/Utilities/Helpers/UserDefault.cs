using System;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.Utils.Interfaces;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class UserDefault
    {

        public static Action IsAuthenticatedChanged;
        
        public static bool IsAuthenticated
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsAuthenticatedUserKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsAuthenticatedUserKey");
                IsAuthenticatedChanged?.Invoke();
            }
        }

        public static bool IsLaunchedBefore
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsLaunchedBeforeKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetBool(value, "IsLaunchedBeforeKey"); }
        }

        public static int PoolingInterval
        {
            get { return (int)NSUserDefaults.StandardUserDefaults.IntForKey("PoolingIntervalKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetInt(value, "PoolingIntervalKey"); }
        }

        public static string RecentsCache
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("RecentsCacheKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetString(value, "RecentsCacheKey"); }
        }

        public static string AccountsCache
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("AccountsCacheKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetString(value, "AccountsCacheKey"); }
        }

        public static string PresentationPhonesCache
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("PresentationPhonesCacheKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetString(value, "PresentationPhonesCacheKey"); }
        }

        public static string LastUsedAccount
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("LastUsedAccountKey"); }
            set { NSUserDefaults.StandardUserDefaults.SetString(value, "LastUsedAccountKey"); }
        }

        public static string AccountPhoneNumber
        {
            get { return NSUserDefaults.StandardUserDefaults.StringForKey("AccountPhoneNumberKey"); }
            set {
                var accountPhoneNumber = ServiceContainer.Resolve<IPhoneFormatter>().Normalize(value);
                NSUserDefaults.StandardUserDefaults.SetString(accountPhoneNumber, "AccountPhoneNumberKey");
            }
        }
    }
}