using System.Net;
using Android.Content;
using FreedomVoice.Core.Cookies;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    public class PclCookieImpl : IDeviceCookieStorage
    {
        private readonly AppPreferencesHelper _helper;

        public PclCookieImpl(Context context)
        {
            _helper = AppPreferencesHelper.Instance(context);
        }

        public CookieContainer GetCookieContainer()
        {
            return _helper.GetCookieContainer();
        }

        public void ClearCookieContainer()
        {
            _helper.ClearCookie();
        }

        public void SaveCookieContainer(CookieContainer cookieContainer)
        {
            _helper.SaveCookie(cookieContainer);
        }
    }
}