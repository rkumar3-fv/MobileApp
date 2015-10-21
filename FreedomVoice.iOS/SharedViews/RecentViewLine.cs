using CoreGraphics;
using Foundation;
using System;
using System.Collections.Generic;
using System.Text;
using UIKit;

namespace FreedomVoice.iOS.SharedViews
{
    [Register("RecentViewLine")]
    public class RecentViewLine : UIView
    {
        public RecentViewLine(CGRect rect) : base (rect)
		{
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            
            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                g.SetLineWidth(1);
                UIColor.Gray.SetStroke();
                UIColor.Clear.SetFill();
                var path = new CGPath();
                path.AddLines(new CGPoint[]{
                    new CGPoint (0, 1),
                    new CGPoint (320, 1)
                });

                path.CloseSubpath();

                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}
