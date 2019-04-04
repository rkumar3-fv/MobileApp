using System;
using CoreGraphics;
using MRoundedButton;
using UIKit;

namespace FreedomVoice.iOS.Views
{
    public class FVRoundedButton: RoundedButton
    {
        public FVRoundedButton(CGRect frame, RoundedButtonStyle style, string identifier) : base(frame, style, identifier)
        {
        }

        public override UIView HitTest(CGPoint point, UIEvent uievent)
        {
            var view = base.HitTest(point, uievent);
            return view;
        }
    }
}
