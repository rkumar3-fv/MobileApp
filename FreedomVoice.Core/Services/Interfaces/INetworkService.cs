using FreedomVoice.Core.Entities.Base;
using FreedomVoice.Entities.Response;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface INetworkService
    {
        /// <summary>
        /// Get conversations from API by provided parameters
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="startDate"></param>
        /// <param name="lastUpdateDate"></param>
        /// <param name="start"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        Task<BaseResult<List<Conversation>>> GetConversations(string phone, DateTime startDate, DateTime lastUpdateDate, int start, int limit);
    }
}
