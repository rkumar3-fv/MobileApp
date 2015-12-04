using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class PoolingIntervalViewModel : BaseViewModel
    {
        private readonly IPollingIntervalService _poolingIntervalService;

        public PoolingIntervalViewModel()
        {
            _poolingIntervalService = ServiceContainer.Resolve<IPollingIntervalService>();
        }

        /// <summary>
        /// Performs an asynchronous Pooling Interval request
        /// </summary>
        /// <returns></returns>
        public async Task GetPoolingIntervalAsync()
        {
            IsBusy = true;

            var requestResult = await _poolingIntervalService.ExecuteRequest();
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as PollingIntervalResponse;
                if (data != null)
                    UserDefault.PoolingInterval = data.PollingInterval;
            }

            IsBusy = false;
        }
    }
}