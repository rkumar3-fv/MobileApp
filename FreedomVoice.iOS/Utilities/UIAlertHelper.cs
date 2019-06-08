using UIKit;

namespace FreedomVoice.iOS.Utilities
{
	public class UIAlertHelper
	{
		private static readonly string Ok = "Ok";
		
		public static void ShowAlert(UIViewController controller, string title, string message)
		{
			var alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create(Ok, UIAlertActionStyle.Default, null));
			controller.PresentViewController(alert, true, null);
		}
	}
}