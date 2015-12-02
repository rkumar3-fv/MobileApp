using System;
using System.Collections.Generic;
using FreedomVoice.Core.Cache;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public class DeviceCacheStorage : IDeviceCacheStorage
    {
        public IEnumerable<string> GetCacheValue(string key)
        {
            var data = GetUserDefaultValueByKey(key);
            return string.IsNullOrEmpty(data) ? new string[] { } : data.Split(',');
        }

        public void SetCacheValue(string key, IEnumerable<string> value)
        {
            var joined = string.Join(",", value);
            SetUserDefaultValueByKey(key, joined);
        }

        public void DeleteCacheValue(string key)
        {
            SetUserDefaultValueByKey(key, string.Empty);
        }

        public void DropCache()
        {
            UserDefault.AccountsCache = string.Empty;
            UserDefault.PresentationPhonesCache = string.Empty;
        }

        private static string GetUserDefaultValueByKey(string key)
        {
            switch (key)
            {
                case CacheStorageClient.AccountsCacheKey:
                    return UserDefault.AccountsCache;
                case CacheStorageClient.PresentationPhonesCacheKey:
                    return UserDefault.PresentationPhonesCache;
                default:
                    return string.Empty;
            }
        }

        private static void SetUserDefaultValueByKey(string key, string value)
        {
            switch (key)
            {
                case CacheStorageClient.AccountsCacheKey:
                    UserDefault.AccountsCache = value;
                    break;
                case CacheStorageClient.PresentationPhonesCacheKey:
                    UserDefault.PresentationPhonesCache = value;
                    break;
            }
        }
    }
}