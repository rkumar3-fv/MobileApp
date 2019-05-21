using System;

namespace FreedomVoice.Core.ViewModels
{
	public class IncomingMessageViewModel : IChatMessage
	{
		public ChatMessageType Type => ChatMessageType.Incoming;

		public string Message { get; }
		public string Time => Date.ToString("t");
		public DateTime Date { get; }
		public readonly long Id;


		public IncomingMessageViewModel(DAL.DbEntities.Message entity)
		{
			Message = entity.Text;
			Id = entity.Id;
			Date = entity.CreatedAt ?? DateTime.Now;
		}
	}
}