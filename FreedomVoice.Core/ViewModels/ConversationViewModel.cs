using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using FreedomVoice.DAL.DbEntities;

namespace FreedomVoice.Core.ViewModels
{
    public class ConversationViewModel
    {
        private readonly string _RawTo;
        public string To => _contactNameProvider.GetName(_RawTo);
        public readonly string Date;
        public readonly string LastMessage;
        public readonly bool IsNew;
        public readonly long ConversationId;
        private readonly IContactNameProvider _contactNameProvider;


        public ConversationViewModel(Conversation entity, IContactNameProvider contactNameProvider)
        {
            _contactNameProvider = contactNameProvider;
            _RawTo = Regex.Replace(entity.ToPhone.PhoneNumber, @"\D", "");
            var message = entity.Messages.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
            if (message == null) return;
            LastMessage = message.Text;
            if (message.CreatedAt != null)
            {
                Date = TimeAgo((DateTime) message.CreatedAt);
                IsNew = message.ReadAt == null;
            }
            else if ( message.SentAt != null )
            {
                Date = TimeAgo((DateTime) message.SentAt);
                IsNew = false;
            }
            ConversationId = entity.Id;
        }
        
        private string TimeAgo(DateTime dateTime)
        {
            string result;
            var timeSpan = DateTime.Now.Subtract(dateTime);

            if (timeSpan <= TimeSpan.FromDays(1))
            {
                result = dateTime.ToString("t");
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