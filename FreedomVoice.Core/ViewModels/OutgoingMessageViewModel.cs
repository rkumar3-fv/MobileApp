using System;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Core.ViewModels
{
    public class OutgoingMessageViewModel : IChatMessage
    {
        public long MessageId { get; }
        public ChatMessageType Type => ChatMessageType.Outgoing;

        public string Message { get; }
        public DateTime Date { get; }
        public SendingState SendingState { get; }
        public string Time => Date.ToLocalTime().ToString("t");

        public OutgoingMessageViewModel(DAL.DbEntities.Message entity)
        {
            Message = entity.Text;
            MessageId = entity.Id;
            Date = entity.CreatedAt ?? DateTime.Now;
            //entity.State == DAL.DbEntities.Enums.SendingState.Sending in this case we alredy have got response form backend
            //but we don't get status from bandwith
            if (entity.State == DAL.DbEntities.Enums.SendingState.Sending ||
                entity.State == DAL.DbEntities.Enums.SendingState.Success)
            {
                SendingState = SendingState.Success;
            }
            else
            {
                SendingState = (SendingState) entity.State;
            }
        }
    }
}