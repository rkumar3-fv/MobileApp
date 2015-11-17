using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class PollingIntervalResponse : BaseResponse
    {
        public int PollingInterval { get; private set; }

        public PollingIntervalResponse(PollingInterval pollingInterval)
        {
            PollingInterval = pollingInterval.PollingIntervalSeconds;
        }
    }
}