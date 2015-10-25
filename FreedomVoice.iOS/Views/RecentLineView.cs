using CoreGraphics;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("RecentLineView")]
    public class RecentLineView : UIView
    {
        public RecentLineView(CGRect rect) : base (rect) { }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            
            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                g.SetLineWidth(1);
                UIColor.Gray.SetStroke();
                UIColor.Clear.SetFill();
                var path = new CGPath();
                path.AddLines(new[] { new CGPoint (0, 1), new CGPoint (320, 1) });

                path.CloseSubpath();

                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.Stroke);
            }
        }
    }
}