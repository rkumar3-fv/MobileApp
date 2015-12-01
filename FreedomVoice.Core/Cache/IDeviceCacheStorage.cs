using System.Collections.Generic;

namespace FreedomVoice.Core.Cache
{
    public interface IDeviceCacheStorage
    {
        IEnumerable<string> GetCacheValue(string key);

        void SetCacheValue(string key, IEnumerable<string> value);

        void DeleteCacheValue(string key);

        void DropCache();
    }
}