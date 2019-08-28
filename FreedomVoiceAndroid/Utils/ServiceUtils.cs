using System;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Lang;
using Exception = Java.Lang.Exception;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class ServiceUtils
    {
        public static bool StartService(Context context, Intent intent)
        {
            try
            {
                if (!App.GetApplication(context).IsAppInForeground && Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    return false;
                context.StartService(intent);
                return true;
            }
            catch (Exception err)
            {
#if DEBUG
                Log.Error(App.AppPackage, err.StackTrace);
#else
                App.GetApplication(context).ApplicationHelper.Reports?.Log(err.StackTrace);
#endif
            }
        }
    }
}