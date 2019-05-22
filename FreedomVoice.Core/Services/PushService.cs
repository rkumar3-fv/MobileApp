using System.Threading.Tasks;
using FreedomVoice.Core.Services.Interfaces;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;

namespace FreedomVoice.Core.Services
{
    public class PushService: IPushService
    {
        
        private readonly INetworkService _networkService;

        public PushService(INetworkService networkService)
        {
            _networkService = networkService;
        }
        
        public async Task<bool> Register(DeviceType deviceType, string token, string systemPhone)
        {
            var request = new PushRequest
            {
                Token = token,
                Type = deviceType,
                SystemPhone = systemPhone
            };
            var res = await _networkService.SendPushToken(request, true);
            return res.Result;
        }

        public async Task<bool> Unregister(DeviceType deviceType, string token, string systemPhone)
        {
            var request = new PushRequest
            {
                Token = token,
                Type = deviceType,
                SystemPhone = systemPhone
            };
            var res = await _networkService.SendPushToken(request, false);
            return res.Result;
        }
    }
}