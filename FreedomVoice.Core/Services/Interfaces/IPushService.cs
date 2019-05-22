using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface IPushService
    {

        /// <summary>
        /// Subscribe device to getting push-notifications
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> Register(DeviceType deviceType, string token, string systemPhone);
        
        /// <summary>
        /// Unsubscribe device to getting push-notifications
        /// </summary>
        /// <param name="deviceType"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task<string> Unregister(DeviceType deviceType, string token, string systemPhone);
    }
}