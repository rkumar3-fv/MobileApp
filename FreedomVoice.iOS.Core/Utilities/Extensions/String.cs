using System;

namespace FreedomVoice.iOS.Core.Utilities.Extensions
{
	public static class String
	{
		public static bool NotNullAndStartsWith(this string text, string textToMatch)
		{
			return !string.IsNullOrEmpty(text) && text.StartsWith(textToMatch, StringComparison.OrdinalIgnoreCase);
		}
	}
}