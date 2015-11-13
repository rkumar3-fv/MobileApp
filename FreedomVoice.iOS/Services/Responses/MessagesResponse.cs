using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.Services.Responses
{
    public class MessagesResponse : BaseResponse
    {
        public List<Message> MessagesList { get; }

        /// <summary>
        /// Response init for MessagesService
        /// </summary>
        /// <param name="messages">Messages</param>
        /// <param name="contactList"></param>
        public MessagesResponse(IEnumerable<Core.Entities.Message> messages, List<Contact> contactList)
        {
            MessagesList = new List<Message>();

            foreach (var m in messages)
                MessagesList.Add(new Message(m, contactList));
        }
    }
}