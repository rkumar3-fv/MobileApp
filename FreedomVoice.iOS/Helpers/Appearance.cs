using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public static class Appearance
    {
        public static UIBarButtonItem GetBackBarButton(UINavigationController controller, string title = "Back")
        {
            var buttonImage = UIImage.FromFile("back.png");
            var button = new UIButton(UIButtonType.Custom) { Frame = new CGRect(0, 0, 120, 33) };
            button.SetImage(buttonImage, UIControlState.Normal);
            button.SetTitle(title, UIControlState.Normal);
            return new UIBarButtonItem(title, UIBarButtonItemStyle.Plain, (s, args) => { controller.PopViewController(true); });
        }

        public static UIBarButtonItem GetLogoutBarButton(UIViewController ctrl)
        {
            return new UIBarButtonItem(UIImage.FromFile("logout.png"), UIBarButtonItemStyle.Plain, (s, args) => {
                var alertController = UIAlertController.Create("Confirm logout?", "Your Recents list will be cleared.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, null));
                alertController.AddAction(UIAlertAction.Create("Log Out", UIAlertActionStyle.Cancel, a => { (UIApplication.SharedApplication.Delegate as AppDelegate).GoToLoginScreen(); }));
                ctrl.PresentViewController(alertController, true, null);
            });
        }
    }
}