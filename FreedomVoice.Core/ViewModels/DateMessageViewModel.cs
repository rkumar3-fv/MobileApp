using System;

namespace FreedomVoice.Core.ViewModels
{
	public class DateMessageViewModel : IChatMessage
	{
		public ChatMessageType Type => ChatMessageType.Date;

		public string Message => Date.ToString("MM/dd/yyyy");
		public string Time => Date.ToString("t");
		public DateTime Date { get; }

		public DateMessageViewModel(DateTime date) {
			Date = date;
		}
	}
}