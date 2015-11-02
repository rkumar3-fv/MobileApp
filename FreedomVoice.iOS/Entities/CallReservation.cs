using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Entities
{
    public class CallReservation
    {
        public CallReservation(CreateCallReservationSetting callReservationSetting)
        {
            SwitchboardNumber = callReservationSetting.SwitchboardPhoneNumber;
        }

        public string SwitchboardNumber { get; set; }
    }
}