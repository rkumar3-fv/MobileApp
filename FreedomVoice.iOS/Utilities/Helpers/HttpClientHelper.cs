using FreedomVoice.Core.Utils;
using ModernHttpClient;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public class HttpClientHelper : IHttpClientHelper
    {
        private NativeMessageHandler _handler;

        public NativeMessageHandler MessageHandler => _handler ?? (_handler = new NativeMessageHandler());
    }
}