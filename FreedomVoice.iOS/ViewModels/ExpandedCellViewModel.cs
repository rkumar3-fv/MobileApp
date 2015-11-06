using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class ExpandedCellViewModel : BaseViewModel
    {
        private readonly IMessageOperationsService _service;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _messageId;

        private static string DestinationFolder => "Trash";

        public string Result { private set; get; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public ExpandedCellViewModel(string systemPhoneNumber, int mailboxNumber, string messageId, UIViewController viewController)
        {
            _service = ServiceContainer.Resolve<IMessageOperationsService>();

            ViewController = viewController;

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
            _messageId = messageId;
        }

        /// <summary>
        /// Asynchronously move messages to Trash folder
        /// </summary>
        /// <returns></returns>
        public async Task MoveMessageToTrashAsync()
        {
            LoadingMessage = "Performing operation...";

            IsBusy = true;

            var requestResult = await _service.ExecuteMoveRequest(_systemPhoneNumber, _mailboxNumber, DestinationFolder, new List<string> { _messageId });
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as MessageOperationsResponse;
                if (data != null)
                    Result = data.Result;
            }

            IsBusy = false;
        }

        /// <summary>
        /// Asynchronously delete messages
        /// </summary>
        /// <returns></returns>
        public async Task DeleteMessageAsync()
        {
            LoadingMessage = "Performing operation...";

            IsBusy = true;

            var requestResult = await _service.ExecuteDeleteRequest(_systemPhoneNumber, _mailboxNumber, new List<string> { _messageId });
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as MessageOperationsResponse;
                if (data != null)
                    Result = data.Result;
            }

            IsBusy = false;
        }
    }
}