using CoreGraphics;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    public class BubbleView: UIView
    {
        public UIColor IncomingColor = new UIColor(red: 0.90f, green: 0.90f, blue: 0.91f, alpha: 1.0f);
        public UIColor OutgoingColor = new UIColor(red: 0.37f, green: 0.81f, blue: 0.36f, alpha: 1.0f);
        public readonly bool IsIncoming;

        public BubbleView(bool isIncoming)
        {
            IsIncoming = isIncoming;
        }
        
        public override void Draw(CGRect rect)
        {
            var width = rect.Width;
            var height = rect.Height;

            var bezierPath = new UIBezierPath();

            if (IsIncoming) {
                bezierPath.MoveTo(new CGPoint( 22,  height));
                bezierPath.AddLineTo(new CGPoint( width - 17,  height));
                bezierPath.AddCurveToPoint(new CGPoint(width, height - 17), new CGPoint(width - 7.61, height),
                    new CGPoint(width, height - 7.61));
                bezierPath.AddLineTo(new CGPoint(width, 17));
                bezierPath.AddCurveToPoint(new CGPoint(width - 17, 0), new CGPoint(width, 7.61), new CGPoint(width - 7.61, 0));
                bezierPath.AddLineTo(new CGPoint(21, 0));;
                bezierPath.AddCurveToPoint(new CGPoint(4, 17), new CGPoint(11.61, 0), new CGPoint(4, 7.61));
                bezierPath.AddLineTo(new CGPoint(4, height - 11));
                bezierPath.AddCurveToPoint(new CGPoint(0, height), new CGPoint(4, height - 1), new CGPoint(0, height));
                bezierPath.AddLineTo(new CGPoint(-0.05, height - 0.01));
                bezierPath.AddCurveToPoint(new CGPoint(11.04, height - 4.04), new CGPoint(4.07, height + 0.43), new CGPoint(8.16, height - 1.06));
                bezierPath.AddCurveToPoint(new CGPoint(22, height), new CGPoint(16, height), new CGPoint(19, height));
    
                IncomingColor.SetFill();
    
            } else
            {
                bezierPath.MoveTo(new CGPoint(width - 22, height));
                bezierPath.AddLineTo(new CGPoint(17, height));
                bezierPath.AddCurveToPoint(new CGPoint(0, height - 17), new CGPoint(7.61, height),
                    new CGPoint(0, height - 7.61));
                bezierPath.AddLineTo(new CGPoint(0, 17));
                bezierPath.AddCurveToPoint(new CGPoint(17, 0), new CGPoint(0, 7.61), new CGPoint(7.61, 0));
                bezierPath.AddLineTo(new CGPoint(width - 21, 0));
                bezierPath.AddCurveToPoint(new CGPoint(width - 4, 17), new CGPoint(width - 11.61, 0),
                    new CGPoint(width - 4, 7.61));
                bezierPath.AddLineTo(new CGPoint(width - 4, height - 11));
                bezierPath.AddCurveToPoint(new CGPoint(width, height), new CGPoint(width - 4, height - 1),
                    new CGPoint(width, height));
                bezierPath.AddLineTo(new CGPoint(width + 0.05, height - 0.01));
                bezierPath.AddCurveToPoint(new CGPoint(width - 11.04, height - 4.04), new CGPoint(width - 4.07, height + 0.43),
                    new CGPoint(width - 8.16, height - 1.06));
                bezierPath.AddCurveToPoint(new CGPoint(width - 22, height), new CGPoint(width - 16, height),
                    new CGPoint(width - 19, height));
    
                OutgoingColor.SetFill();
            }

            bezierPath.ClosePath();
            bezierPath.Fill();
        }
    }
}