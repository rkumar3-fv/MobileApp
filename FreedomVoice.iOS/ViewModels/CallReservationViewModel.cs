using System;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class CallReservationViewModel : BaseViewModel
    {
        private readonly ICallReservationService _service;

        private readonly string _systemNumber;
        private readonly string _callerIdNumber;
        private readonly string _presentationNumber;
        private readonly string _destinationNumber;

        public CallReservation Reservation { get; private set; }

        protected override string LoadingMessage => "Creating Call Reservation...";

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public CallReservationViewModel(string systemNumber, string callerIdNumber, string presentationNumber, string destinationNumber, UIViewController viewController)
        {
            _service = ServiceContainer.Resolve<ICallReservationService>();

            ViewController = viewController;

            _systemNumber = systemNumber;
            _callerIdNumber = callerIdNumber;
            _presentationNumber = presentationNumber;
            _destinationNumber = destinationNumber;
        }

        /// <summary>
        /// Performs an asynchronous CreateCallReservation request
        /// </summary>
        /// <returns></returns>
        public async Task CreateCallReservationAsync()
        {
            IsBusy = true;

            var requestResult = await _service.ExecuteRequest(_systemNumber, _callerIdNumber, _presentationNumber, _destinationNumber);
            if (requestResult is ErrorResponse)
                await ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as CallReservationResponse;
                if (data != null)
                    Reservation = data.Reservation;
            }

            IsBusy = false;
        }
    }
}