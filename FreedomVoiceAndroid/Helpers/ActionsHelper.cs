using Android.Content;
using Android.OS;
using Android.Support.V4.Util;
using com.FreedomVoice.MobileApp.Android.Services;
using Java.Util.Concurrent.Atomic;

namespace com.FreedomVoice.MobileApp.Android.Helpers
{
    /// <summary>
    /// ActionsHelper event delegate
    /// </summary>
    /// <param name="sender">ActionsHelper</param>
    /// <param name="args">ActionsHelperEventArgs</param>
    public delegate void ActionsHelperEvent(object sender, ActionsHelperEventArgs args);

    public class ActionsHelper : IComServiceResultReceiver
    {
        public event ActionsHelperEvent HelperEvent;
        private readonly App _app;
        private readonly ComServiceResultReceiver _receiver;
        private readonly LongSparseArray _requestArray;
        private readonly AtomicLong _idCounter;

        public ActionsHelper(App app)
        {
            _app = app;
            _idCounter = new AtomicLong();
            _requestArray = new LongSparseArray();
            _receiver = new ComServiceResultReceiver(new Handler());
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
            var intent = PrepareIntent(requestId);
            _app.StartService(intent);
            return requestId;
        }

        /// <summary>
        /// Logout action
        /// </summary>
        /// <returns>request ID</returns>
        public long Logout()
        {
            var requestId = RequestId;
            var intent = PrepareIntent(requestId);
            _app.StartService(intent);
            return requestId;
        }

        private Intent PrepareIntent(long requestId)
        {
            var intent = new Intent(_app, typeof(ComService));
            intent.PutExtra(ComService.RequestIdTag, requestId);
            intent.PutExtra(ComServiceResultReceiver.ReceiverTag, _receiver);
            return intent;
        }

        /// <summary>
        /// Responses from ComService
        /// </summary>
        /// <param name="resultCode">result code</param>
        /// <param name="resultData">result data bundle</param>
        public void OnReceiveResult(int resultCode, Bundle resultData)
        {
            
        }
    }
}