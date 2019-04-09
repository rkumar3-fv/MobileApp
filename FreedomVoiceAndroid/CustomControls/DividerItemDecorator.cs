using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;

namespace com.FreedomVoice.MobileApp.Android.CustomControls
{
    /// <summary>
    /// Custom RecyclerView divider
    /// </summary>
    public class DividerItemDecorator : RecyclerView.ItemDecoration
    {
        private readonly Drawable _divider;

        public DividerItemDecorator(Context context)
        {
            var attributes = context.ObtainStyledAttributes(new []{Resource.Attribute.divider});
            _divider = attributes.GetDrawable(0);
            attributes.Recycle();
        }

        public DividerItemDecorator(Context context, int res)
        {
            _divider = ContextCompat.GetDrawable(context, res);
        }

        public override void OnDraw(Canvas c, RecyclerView parent, RecyclerView.State state)
        {
            var left = parent.PaddingLeft;
            var right = parent.Width - parent.PaddingRight;

            var childCount = parent.ChildCount;
            for (var i = 0; i < childCount; i++)
            {
                var child = parent.GetChildAt(i);
                var parameters = child.LayoutParameters.JavaCast<RecyclerView.LayoutParams>();
                var top = child.Bottom + parameters.BottomMargin;

                if (_divider == null) continue;
                var bottom = top + _divider.IntrinsicHeight;
                _divider.SetBounds(left, top, right, bottom);
                _divider.Draw(c);
            }
        }
    }
}