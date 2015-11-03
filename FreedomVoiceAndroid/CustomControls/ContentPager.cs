using System;
using Android.Content;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Util;
using Android.Views;

namespace com.FreedomVoice.MobileApp.Android.CustomControls
{
    public class ContentPager : ViewPager
    {
        /// <summary>
        /// Allowing swipe action
        /// </summary>
        public bool AllowSwipe;

        public ContentPager(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {}

        public ContentPager(Context context) : this(context, false)
        {}

        /// <summary>
        /// ContentPager creation
        /// </summary>
        /// <param name="context">base context</param>
        /// <param name="allowSwipe">allowing swipe action</param>
        public ContentPager(Context context, bool allowSwipe) : base(context)
        {
            AllowSwipe = allowSwipe;
        }

        public ContentPager(Context context, IAttributeSet attrs) : base(context, attrs)
        {}

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            return AllowSwipe && base.OnInterceptTouchEvent(ev);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return AllowSwipe && base.OnTouchEvent(e);
        }
    }
}