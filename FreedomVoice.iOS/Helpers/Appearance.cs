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

        public static UIBarButtonItem GetLogoutBarButton()
        {
            return new UIBarButtonItem("[=>]", UIBarButtonItemStyle.Plain, (s, args) => { (UIApplication.SharedApplication.Delegate as AppDelegate).GoToLoginScreen(); });
        }

        public static void SetDefaultNavigationBarStyle(this UINavigationController controller)
        {
            controller.NavigationBar.TintColor = UIColor.White;
            controller.NavigationBar.TitleTextAttributes = new UIStringAttributes { ForegroundColor = UIColor.White };
            controller.NavigationBar.BarTintColor = new UIColor(0.016f, 0.588f, 0.816f, 1);
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