using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Core.Entities.Enums;

namespace FreedomVoice.Core.Cache
{
    public class CacheStorageClient
    {
        private readonly IDeviceCacheStorage _cacheStorage;

        public const string AccountsCacheKey = "ACCOUNTS";
        public const string PresentationPhonesCacheKey = "PRESENTATIONPHONES";

        public CacheStorageClient(IDeviceCacheStorage cacheStorage)
        {
            _cacheStorage = cacheStorage;
        }

        public async Task<BaseResult<DefaultPhoneNumbers>> GetAccounts()
        {
            return await Task.Run(() => new BaseResult<DefaultPhoneNumbers>
            {
                Code = ErrorCodes.Ok,
                Result = new DefaultPhoneNumbers { PhoneNumbers = _cacheStorage.GetCacheValue(AccountsCacheKey).ToArray() },
                ErrorText = ""
            });
        }

        public async Task<BaseResult<PresentationPhoneNumbers>> GetPresentationPhones()
        {
            return await Task.Run(() => new BaseResult<PresentationPhoneNumbers>
            {
                Code = ErrorCodes.Ok,
                Result = new PresentationPhoneNumbers { PhoneNumbers = _cacheStorage.GetCacheValue(PresentationPhonesCacheKey).ToArray() },
                ErrorText = ""
            });
        }

        public async void SaveAccounts(IEnumerable<string> items)
        {
            await Task.Run(() =>
            {
                _cacheStorage.DropCache();
                _cacheStorage.SetCacheValue(AccountsCacheKey, items);
            });
        }

        public async void SavePresentationPhones(IEnumerable<string> items)
        {
            await Task.Run(() =>
            {
                _cacheStorage.SetCacheValue(PresentationPhonesCacheKey, items);
            });
        }
    }
}