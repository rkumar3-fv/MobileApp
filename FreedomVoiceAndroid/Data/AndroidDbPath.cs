using System;
using System.IO;

namespace com.FreedomVoice.MobileApp.Android.Data
{
    public class AndroidDbPath
    {
        public static string GetDatabasePath(string filename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), filename);
        }
    }
}