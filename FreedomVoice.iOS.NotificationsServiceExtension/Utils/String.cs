using System;

namespace FreedomVoice.iOS.NotificationsServiceExtension.Utils
{
	public static class String
	{
		public static bool NotNullAndStartsWith(this string text, string textToMatch)
		{
			return !string.IsNullOrEmpty(text) && text.StartsWith(textToMatch, StringComparison.OrdinalIgnoreCase);
		}
	}
}