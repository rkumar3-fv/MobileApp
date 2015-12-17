using System.Net;

namespace FreedomVoice.Core.Cookies
{
    public interface IDeviceCookieStorage
    {
        CookieContainer GetCookieContainer();

        void SaveCookieContainer(CookieContainer cookieContainer);

        void ClearCookieContainer();
    }
}