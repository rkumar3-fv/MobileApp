using System.Collections.Generic;

namespace FreedomVoice.Entities
{
    public class Conversation
    {
        public Phone PhoneFrom { get; set; }
        public IEnumerable<Message> Messages { get; set; }
    }
}
