using System.Net;

namespace FreedomVoice.Core.Cookies
{
    public class CookieStorageClient
    {
        private readonly IDeviceCookieStorage _cookieStorage;

        public CookieStorageClient(IDeviceCookieStorage cookieStorage)
        {
            _cookieStorage = cookieStorage;
        }

        public CookieContainer GetCookieContainer()
        {
            return _cookieStorage.GetCookieContainer();
        }

        public void SaveCookieContainer(CookieContainer cookieContainer)
        {
            _cookieStorage.SaveCookieContainer(cookieContainer);
        }

        public void ClearCookieContainer()
        {
            _cookieStorage.ClearCookieContainer();
        }
    }
}