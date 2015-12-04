using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using Xamarin.Contacts;

namespace FreedomVoice.iOS.ViewModels
{
    public class MessagesViewModel : BaseViewModel
    {
        protected override string ResponseName
        {
            get { return "GetMessages"; }
            set { }
        }

        private readonly IMessagesService _service;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _folderName;
        private readonly List<Contact> _contactsList;

        public List<Message> MessagesList { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MessagesViewModel(string systemPhoneNumber, int mailboxNumber, string folderName, List<Contact> contactsList)
        {
            MessagesList = new List<Message>();

            _service = ServiceContainer.Resolve<IMessagesService>();

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
            _folderName = folderName;
            _contactsList = contactsList;
        }

        /// <summary>
        /// Performs an asynchronous Messages request
        /// </summary>
        /// <returns></returns>
        public async Task GetMessagesListAsync(bool silent = false)
        {
            if (PhoneCapability.NetworkIsUnreachable && !silent)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            if (!silent)
                IsBusy = true;

            StartWatcher();

            await RenewCookieIfNeeded();

            var errorResponse = string.Empty;
            var requestResult = await _service.ExecuteRequest(_systemPhoneNumber, _mailboxNumber, _folderName, _contactsList);
            if (requestResult is ErrorResponse)
            {
                if (!silent)
                    errorResponse = ProceedErrorResponse(requestResult);
            }
            else
            {
                var data = requestResult as MessagesResponse;
                if (data != null)
                    MessagesList = data.MessagesList;
            }

            StopWatcher(errorResponse);

            if (!silent)
                IsBusy = false;
        }
    }
}