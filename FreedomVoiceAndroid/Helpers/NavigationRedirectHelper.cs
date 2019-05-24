using System;
using System.Collections.Generic;
using Android.Content;
using com.FreedomVoice.MobileApp.Android.Activities;
using Java.Lang;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    public class NavigationRedirectHelper
    {
        public event EventHandler<IRedirect> OnNewRedirect;
        private readonly Queue<IRedirect> _buffer = new Queue<IRedirect>();

        public void Resume()
        {
            while (_buffer.Count > 0 && OnNewRedirect != null && OnNewRedirect.GetInvocationList().Length > 0)
            {
                OnNewRedirect.Invoke(this, _buffer.Dequeue());
            }
        }

        public void AddRedirect(IRedirect redirect)
        {
            _buffer.Enqueue(redirect);
            Resume();
        }

        public interface IRedirect
        {
            string ScreenKey { get; }
            object Payload { get; }
        }

        public class ActivityRedirect : IRedirect
        {
            public string ScreenKey { get; }
            public object Payload { get; }

            public ActivityRedirect(Intent intent)
            {
                ScreenKey = Screens.ChatScreen;
                Payload = intent;
            }
        }
    }

    class Screens
    {
        public const string ChatScreen = nameof(ChatActivity);
    }
}