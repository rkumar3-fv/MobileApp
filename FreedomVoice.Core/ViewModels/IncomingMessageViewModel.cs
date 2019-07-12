using System;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Core.ViewModels
{
    public class IncomingMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Incoming;
        public long MessageId { get; }

        public string Message { get; }
        public string Time => Date.ToString("t");
        public DateTime Date { get; }

        public SendingState SendingState { get; }

        public IncomingMessageViewModel(DAL.DbEntities.Message entity)
        {
            Message = entity.Text;
            MessageId = entity.Id;
            Date = entity.CreatedAt?.ToLocalTime() ?? DateTime.Now;
            SendingState = (SendingState) entity.State;
        }
    }
}