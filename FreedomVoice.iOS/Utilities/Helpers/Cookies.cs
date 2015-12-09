using System;
using System.Globalization;
using System.Net;
using FreedomVoice.Core;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Cookies
    {
        public static bool IsCookieStored => !IsExpired();

        public static bool HasActiveCookieInContainer()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(new Uri(WebResources.AppUrl));
            var activeCookie = cookies?.Count > 0 ? cookies[0] : null;

            return !IsExpired(activeCookie);
        }

        public static void SaveCookieToStore()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(new Uri(WebResources.AppUrl));
            if (cookies?.Count > 0)
            {
                var cookie = cookies[0];
                UserDefault.RequestCookie = new[] { cookie.Expires.ToString("O"), cookie.Name, cookie.Value, cookie.Path, cookie.Domain };

                return;
            }

            UserDefault.RequestCookie = new string[] { };
        }

        public static void PutStoredCookieToContainer()
        {
            if (ApiHelper.CookieContainer == null)
                ApiHelper.CookieContainer = new CookieContainer();

            ApiHelper.CookieContainer.Add(Cookie);
        }

        private static Cookie Cookie => GetStoredCookie();

        private static bool IsExpired()
        {
            if (UserDefault.RequestCookie.Length == 0)
                return true;

            if (string.IsNullOrEmpty(UserDefault.RequestCookie[0]))
                return true;

            var cookieDateTime = DateTime.ParseExact(UserDefault.RequestCookie[0], "O", CultureInfo.InvariantCulture);

            return cookieDateTime < DateTime.Now;
        }

        private static bool IsExpired(Cookie cookie)
        {
            return cookie == null || cookie.Expires < DateTime.Now;
        }

        private static Cookie GetStoredCookie()
        {
            var cookie = UserDefault.RequestCookie;
            return new Cookie(cookie[1], cookie[2], cookie[3], cookie[4]) { Expires = DateTime.ParseExact(cookie[0], "O", CultureInfo.InvariantCulture) };
        }
    }
}