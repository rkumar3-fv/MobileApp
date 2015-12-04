using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class PoolingIntervalViewModel : BaseViewModel
    {
        protected override string ResponseName
        {
            get { return "GetPoolingInterval"; }
            set { }
        }

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

            StartWatcher();

            var errorResponse = string.Empty;
            var requestResult = await _poolingIntervalService.ExecuteRequest();
            if (requestResult is ErrorResponse)
                errorResponse = ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as PollingIntervalResponse;
                if (data != null)
                    UserDefault.PoolingInterval = data.PollingInterval;
            }

            StopWatcher(errorResponse);

            IsBusy = false;
        }
    }
}