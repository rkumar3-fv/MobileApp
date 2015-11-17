using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class FoldersViewModel : BaseViewModel
    {
        private readonly IFoldersService _service;
        private readonly UIViewController _viewController;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        
        public List<FolderWithCount> FoldersList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public FoldersViewModel(string systemPhoneNumber, int mailboxNumber, UIViewController viewController)
        {
            FoldersList = new List<FolderWithCount>();

            _service = ServiceContainer.Resolve<IFoldersService>();

            ViewController = viewController;
            _viewController = viewController;

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
        }

        /// <summary>
        /// Performs an asynchronous Folders With Count request
        /// </summary>
        /// <returns></returns>
        public async Task GetFoldersListAsync()
        {
            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(_viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            IsBusy = true;

            var requestResult = await _service.ExecuteRequest(_systemPhoneNumber, _mailboxNumber);
            if (requestResult is ErrorResponse)
                await ProceedErrorResponse(requestResult);
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