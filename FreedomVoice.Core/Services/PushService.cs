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
        
        public async Task<bool> Register(DeviceType deviceType, string token)
        {
            var res = await _networkService.SendPushToken(new PushRequest {Token = token, Type = deviceType}, true);
            return res.Result;
        }

        public async Task<bool> Unregister(DeviceType deviceType, string token)
        {
            var res = await _networkService.SendPushToken(new PushRequest {Token = token, Type = deviceType}, false);
            return res.Result;
        }
    }
}