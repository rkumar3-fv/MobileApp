﻿using System.Drawing;
using System.Linq;
using CoreAnimation;
using UIKit;

namespace FreedomVoice.iOS.Helpers
{
    public static class GradientRenderer
    {
        public static void AddLinearGradientToView(this UIView view, UIColor topColor, UIColor bottomColor)
        {
            foreach (var layer in view.Layer.Sublayers.OfType<CAGradientLayer>())
                layer.RemoveFromSuperLayer();

            var gradientLayer = new CAGradientLayer()
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