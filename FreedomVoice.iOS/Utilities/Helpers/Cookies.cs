using System;
using System.Net;
using FreedomVoice.Core;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Cookies
    {
        public static void SaveCookieToStore()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(new Uri(WebResources.AppUrl));
            UserDefault.RequestCookie = cookies?.Count > 0 ? cookies[0].Value : null;
        }

        public static bool IsCookieStored()
        {
            if (CookieIsNotPresentInStorage)
                return false;

            return !Cookie.Expired;
        }

        public static bool IsCookieStored(out Cookie cookie)
        {
            cookie = null;

            if (CookieIsNotPresentInStorage)
                return false;

            cookie = Cookie;
            return !cookie.Expired;
        }

        public static void PrepareCookieFromStore(Cookie cookie)
        {
            ApiHelper.CookieContainer = new CookieContainer();
            ApiHelper.CookieContainer.Add(cookie);
        }

        private static bool CookieIsNotPresentInStorage => string.IsNullOrEmpty(UserDefault.RequestCookie);

        private static Cookie Cookie => new Cookie(".AspNet.ApplicationCookie", UserDefault.RequestCookie, "/", "api.freedomvoice.com");
    }
}