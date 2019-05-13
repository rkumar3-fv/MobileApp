using System;

namespace FreedomVoice.Core.ViewModels
{
	public interface IChatMessage
	{
		ChatMessageType Type { get; }
		string Message { get; }
		string Time { get; }
		DateTime Date { get; }
	}
}