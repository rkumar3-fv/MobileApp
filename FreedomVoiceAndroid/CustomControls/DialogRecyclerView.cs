using Android.Content;
using Android.Support.V7.Widget;
using Android.Util;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.CustomControls
{
    public class DialogRecyclerView : RecyclerView
    {
        private readonly int _size;
        private readonly int _maxHeight;

        public DialogRecyclerView(Context context, int size) : base(context)
        {
            _size = size;
        }

        public DialogRecyclerView(Context context) : this (context, 3)
        {}

        public DialogRecyclerView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            if (attrs != null)
            {
                var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.DialogListMaxHeight);
                _maxHeight = a.GetDimensionPixelSize(Resource.Styleable.DialogListMaxHeight_maxHeight, Integer.MaxValue);
                a.Recycle();
            }
            else
                _maxHeight = 0;
        }

        public DialogRecyclerView(Context context, IAttributeSet attrs, int defStyleAttr) : base (context, attrs, defStyleAttr)
        {
            if (attrs != null)
            {
                var a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.DialogListMaxHeight);
                _maxHeight = a.GetDimensionPixelSize(Resource.Styleable.DialogListMaxHeight_maxHeight, Integer.MaxValue);
                a.Recycle();
            }
            else
                _maxHeight = 0;
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            var measuredHeight = MeasureSpec.GetSize(heightMeasureSpec);
            if (_maxHeight > 0 && _maxHeight < measuredHeight)
            {
                var measureMode = MeasureSpec.GetMode(heightMeasureSpec);
                heightMeasureSpec = MeasureSpec.MakeMeasureSpec(_maxHeight, measureMode);
            }
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
        }
    }
}