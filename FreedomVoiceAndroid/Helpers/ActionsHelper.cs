using Android.OS;
using Android.Support.V4.Util;
using Java.Util.Concurrent.Atomic;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionsHelper event delegate
    /// </summary>
    /// <param name="sender">ActionsHelper</param>
    /// <param name="args">ActionsHelperEventArgs</param>
    public delegate void ActionsHelperEvent(object sender, ActionsHelperEventArgs args);

    public class ActionsHelper
    {
        public event ActionsHelperEvent HelperEvent;
        private readonly App _app;
        private readonly LongSparseArray _requestArray;
        private readonly AtomicLong _idCounter;

        public ActionsHelper(App app)
        {
            _app = app;
            _idCounter = new AtomicLong();
            _requestArray = new LongSparseArray();
        }

        /// <summary>
        /// Get response bundle by request ID
        /// </summary>
        /// <param name="requestId">request ID</param>
        public void GetRusultById(long requestId)
        {
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(requestId, _requestArray.Get(requestId, Bundle.Empty) as Bundle));
        }

        /// <summary>
        /// Remove unused result from history
        /// </summary>
        /// <param name="requetId">request ID</param>
        public void RemoveResult(long requetId)
        {
            _requestArray.Remove(requetId);
        }

        /// <summary>
        /// Get next ID for request
        /// </summary>
        private long RequestId => _idCounter.IncrementAndGet();

        /// <summary>
        /// Authorization action
        /// </summary>
        /// <param name="login">typed login</param>
        /// <param name="password">typed password</param>
        /// <returns>request ID</returns>
        public long Authorize(string login, string password)
        {
            var requestId = RequestId;
            //TODO: run Authorize action in service
            return requestId;
        }
    }
}