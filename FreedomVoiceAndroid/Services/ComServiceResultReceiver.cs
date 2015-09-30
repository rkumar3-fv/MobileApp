using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    public class ComServiceResultReceiver : ResultReceiver
    {
        public const string ReceiverTag = "ComServiceResultReceiver";
        public const string ReceiverDataExtra = "ComServiceResultReceiverExtra";
        public const int ReceiverSuccess = 1;
        public const int ReceiverFailure = 0;

        private IComServiceResultReceiver _receiver;

        /// <summary>
        /// Add receiver listener
        /// </summary>
        /// <param name="listener">listener that implemented IComServiceResultReceiver</param>
        public void SetListener(IComServiceResultReceiver listener)
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

        public ComServiceResultReceiver(Handler handler) : base(handler)
        {}
    }
}