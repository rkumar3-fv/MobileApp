using System;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Core.ViewModels
{
	public class DateMessageViewModel : IChatMessage
	{
		public long MessageId {
            get
            {
                DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                TimeSpan diff = Date - origin;
                return (long)diff.TotalMilliseconds;
            }
        }
		public ChatMessageType Type => ChatMessageType.Date;

		public string Message => Date.ToLocalTime().ToString("MM/dd/yyyy");
		public string Time => Date.ToUniversalTime().ToString("t");
		public DateTime Date { get; }
		public SendingState SendingState { get; }

		public DateMessageViewModel(DateTime date) {
			Date = date;
		}
	}
}