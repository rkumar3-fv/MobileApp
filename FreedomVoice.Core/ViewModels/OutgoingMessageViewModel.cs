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
		public string Time => Date.ToString("t");

		public OutgoingMessageViewModel(DAL.DbEntities.Message entity)
		{
			Message = entity.Text;
			MessageId = entity.Id;
			Date = entity.CreatedAt ?? DateTime.Now;
			SendingState = (SendingState) entity.State;
		}
	}
}