using System;
using System.Threading;
using System.ComponentModel;
using Foundation;

namespace FreedomVoice.iOS.Utilities
{
    /// <summary>
    /// Synchronize invoke implementation for iOS
    /// </summary>
    public class SynchronizeInvoke : NSObject, ISynchronizeInvoke
    {
        /// <summary>
        /// IAsyncResult implementation
        /// </summary>
        class AsyncResult : IAsyncResult
        {
            public object AsyncState { get; set; }

            public WaitHandle AsyncWaitHandle { get; set; }

            public bool CompletedSynchronously => IsCompleted;

            public bool IsCompleted { get; set; }
        }

        public bool InvokeRequired => !NSThread.IsMain;

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            var result = new AsyncResult();

            BeginInvokeOnMainThread(() => {
                result.AsyncWaitHandle = new ManualResetEvent(false);
                result.AsyncState = method.DynamicInvoke(args);
                result.IsCompleted = true;
            });

            return result;
        }

        public object EndInvoke(IAsyncResult result)
        {
            if (!result.IsCompleted)
                result.AsyncWaitHandle.WaitOne();

            return result.AsyncState;
        }

        public object Invoke(Delegate method, object[] args)
        {
            object result = null;
            InvokeOnMainThread(() => result = method.DynamicInvoke(args));
            return result;
        }
    }
}