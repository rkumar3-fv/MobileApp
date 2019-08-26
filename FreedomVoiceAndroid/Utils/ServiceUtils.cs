using System;
using Android.Content;
using Android.Util;
using Java.Lang;
using Exception = Java.Lang.Exception;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class ServiceUtils
    {
        public static void StartService(Context context, Intent intent)
        {
            try
            {
                context.StartService(intent);
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