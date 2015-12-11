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

        public static void SaveCookieToStore()
        {
            var cookieContainer = ApiHelper.CookieContainer;

            var cookies = cookieContainer?.GetCookies(AppUrl);
            if (cookies?.Count > 0)
            {
                UserDefault.RequestCookieExpires = cookies[0].Expires.ToString("O");
                UserDefault.RequestCookie = SerializeCookieContainer(cookieContainer);

                return;
            }

            UserDefault.RequestCookieExpires = string.Empty;
            UserDefault.RequestCookie = string.Empty;
        }

        public static void RestoreCookieFromStore()
        {
            ApiHelper.CookieContainer = GetStoredCookieContainer();
        }

        private static bool IsExpired()
        {
            if (string.IsNullOrEmpty(UserDefault.RequestCookieExpires))
                return true;

            if (string.IsNullOrEmpty(UserDefault.RequestCookie))
                return true;

            var cookieDateTime = DateTime.ParseExact(UserDefault.RequestCookieExpires, "O", CultureInfo.InvariantCulture);

            return cookieDateTime < DateTime.Now;
        }

        private static bool IsExpired(Cookie cookie)
        {
            return cookie == null || cookie.Expires < DateTime.Now;
        }

        private static CookieContainer GetStoredCookieContainer()
        {
            var cookie = UserDefault.RequestCookie;

            return DeserializeCookieContainer(cookie);
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