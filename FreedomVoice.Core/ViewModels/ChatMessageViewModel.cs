using System;
namespace FreedomVoice.Core.ViewModels
{

    public enum ChatMessageType
    { 
        Incoming,
        Outgoing,
        Date
    }

    public interface IChatMessage
    {
        ChatMessageType Type { get; }
        string Message { get; }
    }

    public class IncomingMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Incoming;

        public string Message => _message;
        public readonly long Id;
        private string _message;


        public IncomingMessageViewModel(DAL.DbEntities.Message entity)
        {
            _message = entity.Text;
            Id = entity.Id;
        }
    }

    public class OutgoingMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Outgoing;

        public string Message => _message;
        public readonly long Id;
        private string _message;

        public OutgoingMessageViewModel(DAL.DbEntities.Message entity)
        {
            _message = entity.Text;
            Id = entity.Id;
        }
    }

    public class DateMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Date;

        public string Message => _date.ToString("MM/dd/yyyy");
        private DateTime _date;

        public DateMessageViewModel(DateTime date) {
            _date = date;
        }


    }
   
}
