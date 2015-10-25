using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;

namespace FreedomVoice.iOS.ViewModels
{
    public class FoldersViewModel : BaseViewModel
    {
        readonly IFoldersService _service;
        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;

        public List<FolderWithCount> FoldersList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public FoldersViewModel(string systemPhoneNumber, int mailboxNumber)
        {
            FoldersList = new List<FolderWithCount>();

            _service = ServiceContainer.Resolve<IFoldersService>();

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
        }

        /// <summary>
        /// Performs an asynchronous Folders With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetFoldersListAsync()
        {
            IsBusy = true;

            _service.SetParameters(_systemPhoneNumber, _mailboxNumber);

            var requestResult = await _service.ExecuteRequest();
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as FoldersWithCountResponse;
                if (data != null)
                    FoldersList = data.FoldersWithCount;
            }

            IsBusy = false;
        }
    }
}