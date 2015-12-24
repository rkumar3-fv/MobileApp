using ModernHttpClient;

namespace FreedomVoice.Core.Utils
{
    public interface IHttpClientHelper
    {
        NativeMessageHandler MessageHandler { get; }
    }
}