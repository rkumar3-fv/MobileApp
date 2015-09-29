using System;
using Android.App;
using Android.Content;
using Android.Runtime;

namespace com.FreedomVoice.MobileApp.Android
{
    /// <summary>
    /// Reloaded entry point
    /// </summary>
    [Application
        (Label = "@string/ApplicationName",
        AllowBackup = true,
        Icon = "@mipmap/ic_launcher",
        Theme = "@style/AppTheme")]
    public class App : Application
    {
        /// <summary>
        /// Get app context
        /// </summary>
        public static App GetApplication(Context context)
        {
            var app = context as App;
            if (app != null)
                return app;
            return (App)context.ApplicationContext;
        }

        public App (IntPtr handle, JniHandleOwnership ownerShip) : base(handle, ownerShip)
        { }

        public override void OnCreate()
        {
            base.OnCreate();
        }
    }
}