using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public static class SecurityHelper
    {
        public static string Encrypt(string value, string password, string salt)
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            var algorithm = new TripleDESCryptoServiceProvider();
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);
            var transform = algorithm.CreateEncryptor(rgbKey, rgbIv);
            using (var buffer = new MemoryStream())
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Write))
                {
                    using (var writer = new StreamWriter(stream, Encoding.Unicode))
                    {
                        writer.Write(value);
                    }
                }
                return Convert.ToBase64String(buffer.ToArray());
            }
        }

        public static string Decrypt(string text, string password, string salt)
        {
            DeriveBytes rgb = new Rfc2898DeriveBytes(password, Encoding.Unicode.GetBytes(salt));
            var algorithm = new TripleDESCryptoServiceProvider();
            var rgbKey = rgb.GetBytes(algorithm.KeySize >> 3);
            var rgbIv = rgb.GetBytes(algorithm.BlockSize >> 3);
            var transform = algorithm.CreateDecryptor(rgbKey, rgbIv);
            using (var buffer = new MemoryStream(Convert.FromBase64String(text)))
            {
                using (var stream = new CryptoStream(buffer, transform, CryptoStreamMode.Read))
                {
                    using (var reader = new StreamReader(stream, Encoding.Unicode))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}