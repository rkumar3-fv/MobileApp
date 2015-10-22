using System.Collections.Generic;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class PresentationNumbersResponse : BaseResponse
    {
        public List<PresentationNumber> PresentationNumbers { get; }

        /// <summary>
        /// Response init for PresentationNumbersService
        /// </summary>
        /// <param name="phoneNumbers">Phone Numbers</param>
        public PresentationNumbersResponse(IEnumerable<string> phoneNumbers)
        {
            PresentationNumbers = new List<PresentationNumber>();

            foreach (var phoneNumber in phoneNumbers)
                PresentationNumbers.Add(new PresentationNumber(phoneNumber));
        }
    }
}