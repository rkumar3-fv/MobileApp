using System;

namespace com.FreedomVoice.MobileApp.Android.Adapters
{
    /// <summary>
    /// RecyclerView swipe event arguments
    /// </summary>
    public class SwipeCallbackEventArgs : EventArgs
    {
        /// <summary>
        /// Swipe direction
        /// </summary>
        public int Direction { get; }

        /// <summary>
        /// Swiped element index
        /// </summary>
        public int ElementIndex { get; }

        public SwipeCallbackEventArgs(int direction, int index)
        {
            Direction = direction;
            ElementIndex = index;
        }
    }
}