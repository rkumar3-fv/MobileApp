using System.Collections.Generic;
using System.Linq;
using Android.Content;
using com.FreedomVoice.MobileApp.Android.Entities;
using FreedomVoice.Core.Cache;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    public class PclCacheImpl : IDeviceCacheStorage
    {
        private readonly AppDbHelper _helper;

        public PclCacheImpl(Context context)
        {
            _helper = AppDbHelper.Instance(context);
        }

        public IEnumerable<string> GetCacheValue(string key)
        {
            List<string> res;
            if (key == CacheStorageClient.AccountsCacheKey)
            {
                var accsList = _helper.GetAccounts();
                res = accsList.Select(account => account.AccountName).ToList();
            }
            else
                res = _helper.GetCallerIds();
            return res;
        }

        public void SetCacheValue(string key, IEnumerable<string> value)
        {
            if (key == CacheStorageClient.AccountsCacheKey)
            {
                var tmp = value.Select(accName => new Account(accName, new List<string>())).ToList();
                _helper.InsertAccounts(tmp);
            }
            else
                _helper.InsertPresentationNumbers(value);
        }

        public void DeleteCacheValue(string key)
        {
            if (key == CacheStorageClient.AccountsCacheKey)
                _helper.DropCache();
            else
                _helper.DropCallerIds();
        }

        public void DropCache()
        {
            _helper.DropCache();
        }
    }
}