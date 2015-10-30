using Foundation;

namespace FreedomVoice.iOS.Helpers
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

        public static bool IsLaunchedBefore
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsLaunchedBeforeKey"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsLaunchedBeforeKey");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }
    }
}