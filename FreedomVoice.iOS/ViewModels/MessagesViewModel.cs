using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class MessagesViewModel : BaseViewModel
    {
        private readonly IMessagesService _service;
        private readonly UIViewController _viewController;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _folderName;

        public List<Message> MessagesList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MessagesViewModel(string systemPhoneNumber, int mailboxNumber, string folderName, UIViewController viewController)
        {
            MessagesList = new List<Message>();

            _service = ServiceContainer.Resolve<IMessagesService>();

            ViewController = viewController;
            _viewController = viewController;

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
            _folderName = folderName;
        }

        /// <summary>
        /// Performs an asynchronous Messages request
        /// </summary>
        /// <returns></returns>
        public async Task GetMessagesListAsync(int messageCount, bool silent = false)
        {
            if (PhoneCapability.NetworkIsUnreachable && !silent)
            {
                Appearance.ShowOkAlertWithMessage(_viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            if (!silent)
                IsBusy = true;

            await RenewCookieIfNeeded();

            var requestResult = await _service.ExecuteRequest(_systemPhoneNumber, _mailboxNumber, _folderName, messageCount);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as MessagesResponse;
                if (data != null)
                    MessagesList = data.MessagesList;
            }

            if (!silent)
                IsBusy = false;
        }
    }
}