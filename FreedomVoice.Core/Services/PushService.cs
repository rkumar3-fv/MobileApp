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
        
        public async Task<string> Register(DeviceType deviceType, string token, string systemPhone)
        {
            var res = await _networkService.SendPushToken(new PushRequest {Token = token, Type = deviceType, SystemPhone = systemPhone }, true);
            return res.Result;
        }

        public async Task<string> Unregister(DeviceType deviceType, string token, string systemPhone)
        {
            var res = await _networkService.SendPushToken(new PushRequest {Token = token, Type = deviceType, SystemPhone = systemPhone }, false);
            return res.Result;
        }
    }
}