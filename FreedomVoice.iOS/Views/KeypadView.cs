using System;
using CoreGraphics;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("KeypadView")]
    public class KeypadView : UIView
    {
        public KeypadView(IntPtr handle) : base (handle) { }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            using (CGContext g = UIGraphics.GetCurrentContext())
            {   
                g.SetLineWidth(1);                                                
                UIColor.LightGray.SetStroke();
                UIColor.Clear.SetFill();

                var path = new CGPath();
                path.AddLines(new[] { new CGPoint (0, 103), new CGPoint (320, 103) });

                path.CloseSubpath();
                
                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}