using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class PresentationNumbersViewModel : BaseViewModel
    {
        readonly IPresentationNumbersService _service;
        private readonly string _systemPhoneNumber;

        public List<PresentationNumber> PresentationNumbers;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public PresentationNumbersViewModel(string systemPhoneNumber)
        {
            PresentationNumbers = new List<PresentationNumber>();

            _service = ServiceContainer.Resolve<IPresentationNumbersService>();
            _systemPhoneNumber = systemPhoneNumber;
        }

        /// <summary>
        /// Performs an asynchronous login
        /// </summary>
        /// <returns></returns>
        public async Task GetPresentationNumbersAsync()
        {
            _service.SetSystemNumber(_systemPhoneNumber);

            var requestResult = await _service.ExecuteRequest();
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as PresentationNumbersResponse;
                if (data != null)
                    PresentationNumbers = data.PresentationNumbers;
            }
        }
    }
}