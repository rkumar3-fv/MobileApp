using System.Threading.Tasks;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services
{
    public interface ICallReservationService
    {
        /// <summary>
        /// Asynchronous creation of a call reservation
        /// </summary>
        Task<BaseResponse> ExecuteRequest(string systemNumber, string callerIdNumber, string presentationNumber, string destinationNumber);
    }
}