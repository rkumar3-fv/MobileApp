using System;

namespace com.FreedomVoice.MobileApp.Android.Entities
{
    /// <summary>
    /// Recent entity
    /// </summary>
    public class Recent
    {
        /// <summary>
        /// CallReservation - OK
        /// </summary>
        public const int ResultOk = 1;

        /// <summary>
        /// CallReservation - BadRequest
        /// </summary>
        public const int ResultFail = 0;

        /// <summary>
        /// Destination phone number
        /// </summary>
        public string PhoneNumber { get; }

        /// <summary>
        /// Date of start calling
        /// </summary>
        public DateTime CallDate { get; }

        /// <summary>
        /// Length of call
        /// </summary>
        public int CallLength { get; private set; }

        /// <summary>
        /// Result of GetCallReservation
        /// </summary>
        public int CallResult { get; }

        /// <summary>
        /// Create recent
        /// </summary>
        /// <param name="phoneNumber">Destination phone number</param>
        /// <param name="callResult">CallRequest result</param>
        public Recent(string phoneNumber, int callResult)
        {
            PhoneNumber = phoneNumber;
            CallDate = DateTime.Now;
            CallLength = 0;
            CallResult = callResult;
        }

        /// <summary>
        /// Set call as ended
        /// </summary>
        public void CallEnded()
        {
            if (CallResult == 1)
                CallLength = Convert.ToInt32((DateTime.Now - CallDate).TotalMilliseconds);
        }
    }
}