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
            ServiceRegistrar.Startup();
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
            var preserveDateTimeMethods = DateTime.Now.AddYears(1).AddMonths(1).AddDays(1).AddHours(1).AddMinutes(1).AddSeconds(1);
            string dbPath = IosDbPath.GetDatabasePath("freedomvoice.db");
            var cache = new SQLiteCache(dbPath);
            var conversation = cache.GetConversationById(2);

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}