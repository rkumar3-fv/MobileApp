using System;
using System.Linq;
using System.Net;
using Foundation;
using FreedomVoice.Core;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Cookies
    {
        private static readonly Uri AppUrl = new Uri(WebResources.AppUrl);

        public static bool IsCookieStored => !IsExpired();

        public static void SaveCookieToStore(CookieContainer cookieContainer)
        {
            var cookies = cookieContainer?.GetCookies(AppUrl);
            if (cookies != null && cookies.Count > 0)
                for (var i = 0; i < cookies.Count; i++)
                    NSHttpCookieStorage.SharedStorage.SetCookie(new NSHttpCookie(cookies[i]));
            else
                ClearCookies();
        }

        public static CookieContainer GetStoredCookieContainer()
        {
            var cookieContainer = new CookieContainer();
            var cookies = NSHttpCookieStorage.SharedStorage.CookiesForUrl(AppUrl);
            foreach (var c in cookies)
                cookieContainer.Add(AppUrl, new Cookie(c.Name, c.Value, c.Path, c.Domain) { Expires = NSDateToDateTime(c.ExpiresDate) });

            return cookieContainer;
        }

        public static void ClearCookies()
        {
            var cookies = NSHttpCookieStorage.SharedStorage.CookiesForUrl(AppUrl);
            foreach (var cookie in cookies)
                NSHttpCookieStorage.SharedStorage.DeleteCookie(cookie);
        }

        private static bool IsExpired()
        {
            var cookies = NSHttpCookieStorage.SharedStorage.CookiesForUrl(AppUrl);
            return cookies.Length == 0 || cookies.Any(cookie => NSDateToDateTime(cookie.ExpiresDate).AddDays(-1) < DateTime.Now);
        }

        private static DateTime NSDateToDateTime(NSDate date)
        {
            return TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(2001, 1, 1, 0, 0, 0)).AddSeconds(date.SecondsSinceReferenceDate);
        }
    }
}