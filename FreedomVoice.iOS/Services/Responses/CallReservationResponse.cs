using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class CallReservationResponse : BaseResponse
    {
        public CallReservation Reservation { get; }

        /// <summary>
        /// Response init for CallReservationService
        /// </summary>
        /// <param name="reservationSetting">Call Reservation Setting</param>
        public CallReservationResponse(CreateCallReservationSetting reservationSetting)
        {
            Reservation = new CallReservation(reservationSetting);
        }
    }
}