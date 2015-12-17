using System.Net;
using FreedomVoice.Core.Cookies;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public class DeviceCookieStorage : IDeviceCookieStorage
    {
        public CookieContainer GetCookieContainer()
        {
            return Cookies.IsCookieStored ? Cookies.GetStoredCookieContainer() : null;
        }

        public void SaveCookieContainer(CookieContainer cookieContainer)
        {
            Cookies.SaveCookieToStore(cookieContainer);
        }

        public void ClearCookieContainer()
        {
            Cookies.ClearCookies();
        }
    }
}