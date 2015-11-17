using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface IPollingIntervalService
    {
        /// <summary>
        /// Asynchronously delete messages
        /// </summary>
        Task<BaseResponse> ExecuteRequest();
    }
}