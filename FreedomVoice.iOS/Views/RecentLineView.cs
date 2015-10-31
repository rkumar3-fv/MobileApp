using CoreGraphics;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    [Register("RecentLineView")]
    public class RecentLineView : UIView
    {
        public RecentLineView(CGRect rect) : base(rect)
        {
            BackgroundColor = UIColor.White;
        }

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);

            using (CGContext g = UIGraphics.GetCurrentContext())
            {
                g.SetLineWidth(1f);
                g.MoveTo(0, 0);
                g.SetStrokeColor(UIColor.FromRGB(200, 199, 204).CGColor);
                g.AddLineToPoint(320, 0);
                g.StrokePath();
            }
        }
    }
}