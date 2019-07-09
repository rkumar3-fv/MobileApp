using System;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Core.ViewModels
{
    public interface IChatMessage
    {
        long MessageId { get; }
        ChatMessageType Type { get; }
        string Message { get; }
        string Time { get; }
        DateTime Date { get; }
        SendingState SendingState { get; }
    }
}