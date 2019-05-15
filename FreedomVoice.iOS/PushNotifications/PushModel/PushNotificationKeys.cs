namespace FreedomVoice.iOS.PushNotifications.PushModel
{
	internal enum PushNotificationKeys
	{
		data,
		message,

		conversationId,
		messageId,
		fromPhoneNumber,
		toPhoneNumber
	}
	
	internal static class PushNotificationKeysExtension {
		public static string GetName(this PushNotificationKeys key)
		{
			switch (key)
			{
				case PushNotificationKeys.data:
					return "data";
				case PushNotificationKeys.message:
					return "message";
				case PushNotificationKeys.conversationId:
					return "conversationId";
				case PushNotificationKeys.messageId:
					return "messageId";
				case PushNotificationKeys.fromPhoneNumber:
					return "fromPhoneNumber";
				case PushNotificationKeys.toPhoneNumber:
					return "toPhoneNumber";
				default:
					return "";
			}
		}
	}

}