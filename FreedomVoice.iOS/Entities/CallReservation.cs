using FreedomVoice.Core.Entities;

namespace FreedomVoice.iOS.Entities
{
    public class CallReservation
    {
        public string SwitchboardNumber { get; private set; }

        public CallReservation(CreateCallReservationSetting callReservationSetting)
        {
            SwitchboardNumber = callReservationSetting.SwitchboardPhoneNumber;
        }
    }
}