using CoreGraphics;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("SliderBackgorundView")]
    public class SliderBackgorundView : UIView
    {
        public SliderBackgorundView(CGRect rect) : base (rect) { }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                g.SetLineWidth(14);            
                UIColor.FromRGB(145, 158, 174).SetStroke();
                var path = new CGPath();
                path.AddLines(new[] { new CGPoint(rect.X, rect.Y), new CGPoint(rect.Width, rect.Y) });
                path.CloseSubpath();
                g.AddPath(path);
                g.DrawPath(CGPathDrawingMode.FillStroke);
            }
        }
    }
}