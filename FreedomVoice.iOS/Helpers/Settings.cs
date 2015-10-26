using Foundation;
using FreedomVoice.Core.Utils;

namespace FreedomVoice.iOS.Helpers
{
    public class Settings
    {
        const string accountPhoneNumber = "AccountPhoneNumber";        

        public static void SaveAccountPhoneNumber(string userPhoneNumber)
        {
            NSUserDefaults.StandardUserDefaults.SetString(DataFormatUtils.ToPhoneNumber(userPhoneNumber), accountPhoneNumber);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }
    }
}
