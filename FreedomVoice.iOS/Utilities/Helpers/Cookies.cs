using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using FreedomVoice.Core;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Cookies
    {
        private static readonly Uri AppUrl = new Uri(WebResources.AppUrl);

        public static bool IsCookieStored => !IsExpired();

        public static bool HasActiveCookie()
        {
            var cookies = ApiHelper.CookieContainer?.GetCookies(AppUrl);
            var activeCookie = cookies?.Count > 0 ? cookies[0] : null;

            return !IsExpired(activeCookie);
        }

        public static void SaveCookieToStore(CookieContainer cookieContainer)
        {
            var cookies = cookieContainer?.GetCookies(AppUrl);
            if (cookies == null || cookies.Count == 0)
            {
                ClearCookies();
            }
            else
            {
                UserDefault.RequestCookieExpires = cookies[0].Expires.ToString("O");
                UserDefault.RequestCookie = SerializeCookieContainer(cookieContainer);
            }
        }

        public static CookieContainer GetStoredCookieContainer()
        {
            return DeserializeCookieContainer(UserDefault.RequestCookie);
        }

        public static void ClearCookies()
        {
            UserDefault.RequestCookie = string.Empty;
            UserDefault.RequestCookieExpires = string.Empty;
        }

        private static bool IsExpired()
        {
            if (string.IsNullOrEmpty(UserDefault.RequestCookieExpires))
                return true;

            if (string.IsNullOrEmpty(UserDefault.RequestCookie))
                return true;

            return DateTime.ParseExact(UserDefault.RequestCookieExpires, "O", CultureInfo.InvariantCulture).AddDays(-1) < DateTime.Now;
        }

        private static bool IsExpired(Cookie cookie)
        {
            return cookie == null || cookie.Expires.AddDays(-1) < DateTime.Now;
        }

        private static string SerializeCookieContainer(CookieContainer container)
        {
            using (var memoryStream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, container);

                return Convert.ToBase64String(memoryStream.ToArray());
            }
        }

        private static CookieContainer DeserializeCookieContainer(string serializedCookieContainer)
        {
            var cookieContainerBytes = Convert.FromBase64String(serializedCookieContainer);
            using (var memoryStream = new MemoryStream(cookieContainerBytes))
            {
                var formatter = new BinaryFormatter();
                return (CookieContainer)formatter.Deserialize(memoryStream);
            }
        }
    }
}