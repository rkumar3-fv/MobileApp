using Foundation;

namespace FreedomVoice.iOS.Helpers
{
    public static class UserDefault
    {
        public static bool IsAuthenticated
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("IsAuthenticatedUser"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "IsAuthenticatedUser");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }

        public static bool DisclaimerWasShown
        {
            get { return NSUserDefaults.StandardUserDefaults.BoolForKey("DisclaimerWasShown"); }
            set
            {
                NSUserDefaults.StandardUserDefaults.SetBool(value, "DisclaimerWasShown");
                NSUserDefaults.StandardUserDefaults.Synchronize();
            }
        }
    }
}