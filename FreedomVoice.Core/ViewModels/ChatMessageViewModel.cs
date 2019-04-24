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
        private static string TimeFormat = "t";
        ChatMessageType Type { get; }
        string Message { get; }
        string Time { get; }
        DateTime Date { get; }
    }

    public class IncomingMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Incoming;

        public string Message => _message;
        public string Time => Date.ToString("t");
        public DateTime Date { get; }
        public readonly long Id;
        private string _message;


        public IncomingMessageViewModel(DAL.DbEntities.Message entity)
        {
            _message = entity.Text;
            Id = entity.Id;
            Date = entity.ReceivedAt ?? DateTime.Now;
        }
    }

    public class OutgoingMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Outgoing;

        public string Message => _message;
        public DateTime Date { get; }
        public string Time => Date.ToString("t");
        public readonly long Id;
        private string _message;

        public OutgoingMessageViewModel(DAL.DbEntities.Message entity)
        {
            _message = entity.Text;
            Id = entity.Id;
            Date = entity.ReceivedAt ?? DateTime.Now;
        }
    }

    public class DateMessageViewModel : IChatMessage
    {
        public ChatMessageType Type => ChatMessageType.Date;

        public string Message => _date.ToString("MM/dd/yyyy");
        public string Time => Date.ToString("t");
        public DateTime Date => _date;
        private DateTime _date;

        public DateMessageViewModel(DateTime date) {
            _date = date;
        }


    }
   
}
