using Android.Content;
using Android.Util;
using Android.Widget;

namespace com.FreedomVoice.MobileApp.Android.CustomControls
{
    public class AlwaysTopSpinner : Spinner
    {
        private bool _toggleFlag = true;

        public AlwaysTopSpinner(Context context, IAttributeSet attrs, int defStyle, SpinnerMode mode) 
            : base (context, attrs, defStyle, mode)
        {}

        public AlwaysTopSpinner(Context context, IAttributeSet attrs, int defStyle):
            base (context, attrs, defStyle)
        {}

        public AlwaysTopSpinner(Context context, IAttributeSet attrs) : base (context, attrs)
        {}

        public AlwaysTopSpinner(Context context, SpinnerMode mode): base(context, mode)
        {}

        public AlwaysTopSpinner(Context context) : base(context)
        {}

        public override int SelectedItemPosition => !_toggleFlag ? 0 : base.SelectedItemPosition;

        public override bool PerformClick()
        {
            _toggleFlag = false;
            var result = base.PerformClick();
            _toggleFlag = true;
            return result;
        }
    }
}