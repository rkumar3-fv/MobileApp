using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class CookieSerializer
    {
        public static void ClearCookies(string file)
        {
            try
            {
                if (File.Exists(file))
                    File.Delete(file);
            }
            catch (Exception)
            {
                //ignored
            }
            
        }

        public static void WriteCookiesToDisk(string file, CookieContainer cookieJar)
        {
            ClearCookies(file);
            using (Stream stream = File.Create(file))
            {
                try
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, cookieJar);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        public static CookieContainer ReadCookiesFromDisk(string file)
        {
            try
            {
                using (Stream stream = File.Open(file, FileMode.Open))
                {
                    var formatter = new BinaryFormatter();
                    return (CookieContainer)formatter.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}