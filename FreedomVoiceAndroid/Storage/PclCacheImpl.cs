using System.Collections.Generic;
using Android.Content;

namespace com.FreedomVoice.MobileApp.Android.Storage
{
    public class PclCacheImpl
    {
        private readonly AppDbHelper _helper;

        public PclCacheImpl(Context context)
        {
            _helper = AppDbHelper.Instance(context);
        }

        public void SaveInCache(string key, IEnumerable<string> values)
        {
            
        }

        public IEnumerable<string> LoadFromCache(string key)
        {
            return new List<string>();
        }

        public void DropCache()
        {
            _helper.DropCache();
        }

        public void DeleteByKey(string key)
        {
            _helper.DropCache();
        }
    }
}