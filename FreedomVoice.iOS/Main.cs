using FreedomVoice.Core.Cache;
using FreedomVoice.iOS.Data;
using FreedomVoice.iOS.Utilities;
using System;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;
using FreedomVoice.iOS.PushNotifications;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS
{
    public static class Application
    {
        static void Main(string[] args)
        {
            SQLitePCL.raw.SetProvider(new SQLitePCL.SQLite3Provider_sqlite3());
            
            //This workaround was done because EntityFramework for SQLite has known issues with Xamarin iOS.
            var preserveDateTimeMethods = DateTime.Now.AddYears(1).AddMonths(1).AddDays(1).AddHours(1).AddMinutes(1).AddSeconds(1);
            
            ServiceRegistrar.Startup();
            ServiceContainer.Register<IContactNameProvider>(() => new ContactNameProvider());
            ServiceContainer.Register<IPushNotificationTokenDataStore>(() => new PushNotificationTokenDataStore());
            ServiceContainer.Register<IPushNotificationsService>(() => new PushNotificationsService());
            ServiceContainer.Register<IAppNavigator>(() => new AppNavigator());

            UIApplication.Main(args, null, "AppDelegate");
        }
    }
}