using Android.Content;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.Widget;
using Android.Support.V7.Widget.Helper;
using com.FreedomVoice.MobileApp.Android.CustomControls.CustomEventArgs;

namespace com.FreedomVoice.MobileApp.Android.CustomControls.Callbacks
{
    public delegate void SwipeCallbackEvent(object sender, SwipeCallbackEventArgs args);

    /// <summary>
    /// RecyclerView swipe listener
    /// </summary>
    public class SwipeCallback : ItemTouchHelper.SimpleCallback
    {
        /// <summary>
        /// Swipe event
        /// </summary>
        public event SwipeCallbackEvent SwipeEvent;

        private readonly Context _context;
        private readonly int _colorResRight;
        private readonly int _drawableResRight;
        private readonly int _colorResLeft;
        private readonly int _drawableResLeft;

        public SwipeCallback(int dragDirs, int swipeDirs, Context context, int colorResRight, int drawableRight, int colorResLeft, int drawableLeft) : base(dragDirs, swipeDirs)
        {
            _context = context;
            _colorResRight = colorResRight;
            _drawableResRight = drawableRight;
            _colorResLeft = colorResLeft;
            _drawableResLeft = drawableLeft;
        }

        public SwipeCallback(int dragDirs, int swipeDirs, Context context, int colorResRight, int drawableRight) : this(dragDirs, swipeDirs, context, colorResRight, drawableRight, colorResRight, drawableRight)
        { }

        public override bool OnMove(RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, RecyclerView.ViewHolder target)
        {
            return false;
        }

        public override void OnSwiped(RecyclerView.ViewHolder viewHolder, int direction)
        {
            SwipeEvent?.Invoke(this, new SwipeCallbackEventArgs(direction, viewHolder.AdapterPosition));
        }

        public override void OnChildDraw(Canvas cValue, RecyclerView recyclerView, RecyclerView.ViewHolder viewHolder, float dX, float dY, int actionState,
            bool isCurrentlyActive)
        {
            if (actionState == ItemTouchHelper.ActionStateSwipe)
            {
                var itemView = viewHolder.ItemView;
                var paint = new Paint();
                Bitmap bitmap;

                if (dX > 0)
                {
                    paint.Color = new Color(ContextCompat.GetColor(_context, _colorResRight));
                    bitmap = BitmapFactory.DecodeResource(_context.Resources, _drawableResRight);
                    float height = (itemView.Height/2) - (bitmap.Height/2);

                    cValue.DrawRect(itemView.Left, itemView.Top, dX, itemView.Bottom, paint);
                    cValue.DrawBitmap(bitmap, 16f, itemView.Top + height, null);
                }
                else
                {
                    paint.Color = new Color(ContextCompat.GetColor(_context, _colorResLeft));
                    bitmap = BitmapFactory.DecodeResource(_context.Resources, _drawableResLeft);
                    float height = (itemView.Height/2) - (bitmap.Height/2);
                    float bitmapWidth = bitmap.Width;

                    cValue.DrawRect(itemView.Right + dX, itemView.Top, itemView.Right, itemView.Bottom, paint);
                    cValue.DrawBitmap(bitmap, (itemView.Right - bitmapWidth) - 16f, itemView.Top + height, null);
                }
            }
            base.OnChildDraw(cValue, recyclerView, viewHolder, dX, dY, actionState, isCurrentlyActive);
        }
    }
}