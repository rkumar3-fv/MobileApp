using System;
using System.Drawing;

using CoreGraphics;
using Foundation;
using UIKit;

namespace FreedomVoice.iOS.SharedViews
{
    [Register("CallerIdView")]
    public class CallerIdView : UIView
    {
        public CallerIdView()
        {
            Initialize();
        }

        public CallerIdView(RectangleF bounds) : base(bounds)
        {
            Initialize();
        }

        void Initialize()
        {
            var labelField = new UILabel(new CGRect(15, 0, 145, 44)) { Text = "Show as Caller ID:" };
            var dropdownField = new UIPickerView(new CGRect(160, 0, 160, 44));
            AddSubviews(labelField, dropdownField);
        }
    }
}