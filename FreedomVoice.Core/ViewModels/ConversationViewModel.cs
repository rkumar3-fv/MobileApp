using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using FreedomVoice.DAL.DbEntities;

namespace FreedomVoice.Core.ViewModels
{
    public class ConversationViewModel
    {
        private readonly string _RawCollocutor;
        public string Collocutor => _contactNameProvider.GetName(_RawCollocutor);
        public readonly string Date;
        public readonly string LastMessage;
        public readonly bool IsNew;
        private readonly IContactNameProvider _contactNameProvider;


        public ConversationViewModel(Conversation entity, IContactNameProvider contactNameProvider)
        {
            _contactNameProvider = contactNameProvider;
            _RawCollocutor = Regex.Replace(entity.CollocutorPhone.PhoneNumber, @"\D", "");
            var message = entity.Messages.FirstOrDefault();
            if (message == null) return;
            LastMessage = message.Text;
            var from = Regex.Replace(message.From.PhoneNumber, @"\D", "");
            //last message not from us
            if (from.Equals(_RawCollocutor) && message.ReceivedAt != null)
            {
                Date = TimeAgo((DateTime) message.ReceivedAt);
                IsNew = message.ReadAt == null;
            }
            else if ( message.SentAt != null )
            {
                Date = TimeAgo((DateTime) message.SentAt);
                IsNew = false;
            }
        }
        
        private string TimeAgo(DateTime dateTime)
        {
            string result;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromDays(1))
            {
                result = dateTime.ToString("HH:mm");
            }
            else if (timeSpan <= TimeSpan.FromDays(7))
            {
                result = dateTime.ToString("dddd");
            }
            else
            {
                result = dateTime.ToString("MM/dd/yyyy");
            }

            return result;
        }
    }
}