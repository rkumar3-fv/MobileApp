using System.Collections.Generic;
using System.Linq;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Response;

namespace FreedomVoice.Core.Utils.Extensions
{
    public static class ConversationExtensions
    {
        public static List<Phone> AllPhones(this Conversation conversation)
        {
            var phones = new Dictionary<long, Phone>();
            if (conversation.SystemPhone != null)
            {
                phones[conversation.SystemPhone.Id] = conversation.SystemPhone;
            }
            if (conversation.ToPhone != null)
            {
                phones[conversation.ToPhone.Id] = conversation.ToPhone;
            }

            foreach (var message in conversation.Messages)
            {
                if (message.To != null)
                {
                    phones[message.To.Id] = message.To;
                }
                if (message.From != null)
                {
                    phones[message.From.Id] = message.From;
                }
            }
            return phones.Values.ToList();
        }
    }
}