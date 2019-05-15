namespace FreedomVoice.iOS.PushNotifications.PushModel
{
	public class Message
	{
		public long conversationId { get; set; }
		public long messageId { get; set; }
		public string fromPhoneNumber { get; set; }
		public string toPhoneNumber { get; set; }
	}
}