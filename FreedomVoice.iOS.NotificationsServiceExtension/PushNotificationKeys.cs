using System;

namespace FreedomVoice.iOS.NotificationsServiceExtension
{
	enum PushNotificationKeys
	{
		phone
	}
	
	static class PushNotificationKeysExtension {
		public static string GetName(this PushNotificationKeys key)
		{
			switch (key)
			{
				case PushNotificationKeys.phone:
					return "phone";
				default:
					throw new ArgumentOutOfRangeException(nameof(key), key, null);
			}
		}
	}
}