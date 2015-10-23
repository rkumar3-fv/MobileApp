using System.Drawing;
using System.Linq;
using CoreAnimation;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public static class Appearance
    {
        public static UIBarButtonItem GetBackBarButton(UINavigationController controller, string title = "Back")
        {
            return new UIBarButtonItem(title, UIBarButtonItemStyle.Plain, (s, args) => { controller.PopViewController(true); });
        }

        public static UIBarButtonItem GetLogoutBarButton(UIViewController ctrl)
        {
            return new UIBarButtonItem("Logout", UIBarButtonItemStyle.Plain, (s, args) => {
                var alertController = UIAlertController.Create("Confirm logout?", "Your Recents list will be cleared.", UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Default, a => {}));
                alertController.AddAction(UIAlertAction.Create("Log Out", UIAlertActionStyle.Cancel, a => { (UIApplication.SharedApplication.Delegate as AppDelegate).GoToLoginScreen(); }));
                ctrl.PresentViewController(alertController, true, null);
            });
        }

        public static void AddLinearGradientToView(this UIView view, UIColor topColor, UIColor bottomColor)
        {
            foreach (var layer in view.Layer.Sublayers.OfType<CAGradientLayer>())
                layer.RemoveFromSuperLayer();

            var gradientLayer = new CAGradientLayer
            {
                StartPoint = new PointF { X = 0.5f, Y = 0 },
                EndPoint = new PointF { X = 0.5f, Y = 1 },
                Frame = view.Bounds,
                Colors = new[] { topColor.CGColor, bottomColor.CGColor }
            };

            if (view.Layer.Sublayers.Length > 0)
            {
                view.Layer.InsertSublayer(gradientLayer, 0);
            }
            else
            {
                view.Layer.AddSublayer(gradientLayer);
            }
        }
    }
}