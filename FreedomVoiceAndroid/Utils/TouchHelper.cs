using Android.Graphics;
using Android.Runtime;
using Android.Views;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class TouchHelper
    {
        public static void IncreaseClickArea(View parent, View child)
        {
            var childLocal = child;
            parent.Post(() =>
            {
                var delegateArea = new Rect();
                var delegateView = childLocal;
                delegateView.GetHitRect(delegateArea);
                delegateArea.Top -= 100;
                delegateArea.Bottom += 100;
                delegateArea.Left -= 50;
                delegateArea.Right += 50;
                var expandedArea = new TouchDelegate(delegateArea, delegateView);
                if (!(delegateView.Parent is View)) return;
                var parentView = delegateView.Parent.JavaCast<View>();
                parentView.TouchDelegate = expandedArea;
            });
        }
    }
}