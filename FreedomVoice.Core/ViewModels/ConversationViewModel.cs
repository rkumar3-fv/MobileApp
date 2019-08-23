using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.DAL.DbEntities;

namespace FreedomVoice.Core.ViewModels
{
    public class ConversationViewModel
    {
        private readonly string _RawTo;
        public string To => _contactNameProvider.GetName(_RawTo);
        public readonly string Date;
        public readonly DateTime DateTime;
        public readonly string LastMessage;
        public  bool IsNew;
        public readonly long ConversationId;
        private readonly IContactNameProvider _contactNameProvider;


        public ConversationViewModel(Conversation entity, IContactNameProvider contactNameProvider,
            IPhoneFormatter formatter)
        {
            _contactNameProvider = contactNameProvider;

            _RawTo = formatter.NormalizeNational(entity.ToPhone.PhoneNumber);
            var message = entity.Messages.OrderByDescending(x => x.OrderDate).FirstOrDefault();
            if (message == null) return;
            LastMessage = message.Text;
            Date = TimeAgo((DateTime)message.OrderDate);
            DateTime = (DateTime)message.OrderDate;

            if (!message.From.PhoneNumber.Equals(entity.SystemPhone.PhoneNumber))
            {
                IsNew = message.ReadAt == null;
            }
            else
            {
                IsNew = false;
            }
            ConversationId = entity.Id;
        }

        private string TimeAgo(DateTime dateTime)
        {
            string result;
            var nowDate = DateTime.Now;
            var timeSpan = nowDate.ToUniversalTime().Subtract(dateTime);
            if (timeSpan <= TimeSpan.FromDays(1) && dateTime.Day == nowDate.Day)
            {
                result = dateTime.ToLocalTime().ToString("t");
            }
            else if (timeSpan <= TimeSpan.FromDays(7))
            {
                result = dateTime.ToLocalTime().ToString("dddd");
            }
            else
            {
                result = dateTime.ToLocalTime().ToString("MM/dd/yyyy");
            }

            return result;
        }
    }
}