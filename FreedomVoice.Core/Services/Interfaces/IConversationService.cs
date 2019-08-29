using FreedomVoice.Core.Entities.Texting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FreedomVoice.Core.Services.Interfaces
{
    public interface IConversationService
    {
        /// <summary>
        /// Get list of conversations filtered by provided parameters
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="current"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<ConversationListResponse> GetList(string systemPhoneNumber, string presentationPhoneNumber, DateTime current, int count = 10, int page = 1);

        /// <summary>
        /// Search list of conversation by provided parameters
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="query"></param>
        /// <param name="foundInNumbers"></param>
        /// <param name="current"></param>
        /// <param name="count"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<ConversationListResponse> Search(string systemPhoneNumber, string presentationPhoneNumber, string query, string[] foundInNumbers, DateTime current, int count = 10, int page = 1);

        /// <summary>
        /// Get conversation by system phone number and to phone
        /// </summary>
        /// <param name="systemPhoneNumber"></param>
        /// <param name="presentationPhoneNumber"></param>
        /// <param name="toPhone"></param>
        /// <returns></returns>
        Task<ConversationResponse> Get(string systemPhoneNumber, string presentationPhoneNumber, string toPhone);
    }
}
