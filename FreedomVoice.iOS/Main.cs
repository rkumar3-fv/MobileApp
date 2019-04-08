using FreedomVoice.Core.Cache;
using FreedomVoice.iOS.Data;
using FreedomVoice.iOS.Utilities;
using System;
using UIKit;

namespace FreedomVoice.iOS
{
    public static class Application
    {
        static void Main(string[] args)
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
            var preserveDateTimeMethods = DateTime.Now.AddYears(1).AddMonths(1).AddDays(1).AddHours(1).AddMinutes(1).AddSeconds(1);
            ServiceRegistrar.Startup();

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}