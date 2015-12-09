using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class CookieHelper
    {
        public static List<Cookie> GetAllCookies(CookieContainer container)
        {
            var cookies = new List<Cookie>();
            var table = (Hashtable)container.GetType().InvokeMember("m_domainTable",
                                                                    BindingFlags.NonPublic |
                                                                    BindingFlags.GetField |
                                                                    BindingFlags.Instance,
                                                                    null,
                                                                    container,
                                                                    new object[] { });
            foreach (var key in table.Keys)
            {
                Uri uriHttp;
                Uri uriHttps;
                var domain = key as string;
                if (domain == null)
                    continue;
                if (domain.StartsWith("."))
                    domain = domain.Substring(1);

                var addressHttp = $"http://{domain}/";
                var addressHttps = $"https://{domain}/";
                var resHttp = Uri.TryCreate(addressHttp, UriKind.RelativeOrAbsolute, out uriHttp);
                var resHttps = Uri.TryCreate(addressHttps, UriKind.RelativeOrAbsolute, out uriHttps);
                if (resHttps)
                    cookies.AddRange(container.GetCookies(uriHttps).Cast<Cookie>());
                if (resHttp)
                    cookies.AddRange(container.GetCookies(uriHttp).Cast<Cookie>());
            }
            return cookies;
        }
    }
}