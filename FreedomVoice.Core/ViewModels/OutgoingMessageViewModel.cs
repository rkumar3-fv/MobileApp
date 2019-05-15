using System;

namespace FreedomVoice.Core.ViewModels
{
	public class OutgoingMessageViewModel : IChatMessage
	{
		public ChatMessageType Type => ChatMessageType.Outgoing;

		public string Message { get; }
		public DateTime Date { get; }
		public string Time => Date.ToString("t");
		public readonly long Id;

		public OutgoingMessageViewModel(DAL.DbEntities.Message entity)
		{
			Message = entity.Text;
			Id = entity.Id;
			Date = entity.CreatedAt ?? DateTime.Now;
		}
	}
}