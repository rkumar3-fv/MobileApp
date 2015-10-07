using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using com.FreedomVoice.MobileApp.Android.Actions.Requests;
using com.FreedomVoice.MobileApp.Android.Actions.Responses;
using com.FreedomVoice.MobileApp.Android.Services;
using Java.Util.Concurrent.Atomic;
using LongSparseArray = Android.Support.V4.Util.LongSparseArray;

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
            _receiver.SetListener(this);
        }

        /// <summary>
        /// Get response by request ID
        /// </summary>
        /// <param name="requestId">request ID</param>
        public void GetRusultById(long requestId)
        {
            Toast.MakeText(_app, requestId.ToString(),ToastLength.Short).Show();
            HelperEvent?.Invoke(this, new ActionsHelperEventArgs(requestId, _requestArray.Get(requestId, null) as BaseResponse));
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
            intent.PutExtra(ComService.RequestTag, new LoginRequest(requestId, login, password));
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
            intent.PutExtra(ComService.RequestTag, new LogoutRequest(requestId));
            _app.StartService(intent);
            return requestId;
        }

        /// <summary>
        /// Restore password action
        /// </summary>
        /// <param name="email">e-mail</param>
        /// <returns>request ID</returns>
        public long RestorePassword(string email)
        {
            var requestId = RequestId;
            var intent = PrepareIntent(requestId);
            intent.PutExtra(ComService.RequestTag, new RestorePasswordRequest(requestId, email));
            _app.StartService(intent);
            return requestId;
        }

        /// <summary>
        /// Prepare intent for request
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns>prepaired intent</returns>
        private Intent PrepareIntent(long requestId)
        {
            _requestArray.Put(requestId, null);
            var intent = new Intent(_app, typeof(ComService));
            intent.SetAction(ComService.ExecuteAction);
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
            if (resultCode != (int)Result.Ok) return;
            if (HelperEvent == null) return;
            var response = resultData.GetParcelable(ComServiceResultReceiver.ReceiverDataExtra) as BaseResponse;
            if (response == null) return;
            _requestArray.Put(response.RequestId, response);
            HelperEvent.Invoke(this, new ActionsHelperEventArgs(response.RequestId, response));
        }
    }
}