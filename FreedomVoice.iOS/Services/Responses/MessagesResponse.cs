using System.Collections.Generic;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class MessagesResponse : BaseResponse
    {
        public List<Message> MessagesList { get; }

        /// <summary>
        /// Response init for MessagesService
        /// </summary>
        /// <param name="messages">Messages</param>
        public MessagesResponse(IEnumerable<FreedomVoice.Core.Entities.Message> messages)
        {
            MessagesList = new List<Message>();

            foreach (var m in messages)
                MessagesList.Add(new Message(m));
        }
    }
}