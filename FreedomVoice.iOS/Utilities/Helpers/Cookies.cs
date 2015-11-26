using System;
using System.Net;
using FreedomVoice.Core;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Cookies
    {
        public static bool IsCookieStored => !(string.IsNullOrEmpty(UserDefault.RequestCookie) || Cookie.Expired);

        public static bool HasActiveCookieInContainer()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(new Uri(WebResources.AppUrl));
            var activeCookie = cookies?.Count > 0 ? cookies[0] : null;

            return activeCookie != null && !activeCookie.Expired;
        }

        public static void SaveCookieToStore()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(new Uri(WebResources.AppUrl));
            UserDefault.RequestCookie = cookies?.Count > 0 ? cookies[0].Value : string.Empty;
        }

        public static void PutStoredCookieToContainer()
        {
            ApiHelper.CookieContainer = new CookieContainer();
            ApiHelper.CookieContainer.Add(Cookie);
        }

        private static Cookie Cookie => new Cookie(".AspNet.ApplicationCookie", UserDefault.RequestCookie, "/", "api.freedomvoice.com");
    }
}