using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    public abstract class AppServiceResultReceiver : ResultReceiver
    {
        private IAppServiceResultReceiver _receiver;

        /// <summary>
        /// Add receiver listener
        /// </summary>
        /// <param name="listener">listener that implemented IAppServiceResultReceiver</param>
        public void SetListener(IAppServiceResultReceiver listener)
        {
            _receiver = listener;
        }

        /// <summary>
        /// Remove receiver listener
        /// </summary>
        public void RemoveListener()
        {
            _receiver = null;
        }

        /// <summary>
        /// Callback running
        /// </summary>
        /// <param name="resultCode">result code</param>
        /// <param name="resultData">result data bundle</param>
        protected override void OnReceiveResult(int resultCode, Bundle resultData)
        {
            _receiver?.OnReceiveResult(resultCode, resultData);
        }

        protected AppServiceResultReceiver(Handler handler) : base(handler)
        {}
    }
}