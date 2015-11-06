using Android.OS;

namespace com.FreedomVoice.MobileApp.Android.Services
{
    /// <summary>
    /// Communication service result receiver interface
    /// </summary>
    public interface IAppServiceResultReceiver
    {
        /// <summary>
        /// Result callback
        /// </summary>
        /// <param name="resultCode">result code (see ComServiceResultReceiver consts)</param>
        /// <param name="resultData">result data bundle</param>
        void OnReceiveResult(int resultCode, Bundle resultData);
    }
}