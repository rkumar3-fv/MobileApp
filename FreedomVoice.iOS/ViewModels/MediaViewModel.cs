using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class MediaViewModel : BaseViewModel
    {
        protected override string ResponseName
        {
            get { return "GetMedia"; }
            set { }
        }

        private readonly IMediaService _service;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _folderName;
        private readonly string _messageId;
        private readonly MediaType _mediaType;

        public string FilePath { get; private set; }

        protected override ProgressControlType ProgressControl => ProgressControlType.ProgressBar;

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MediaViewModel(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType)
        {
            _service = ServiceContainer.Resolve<IMediaService>();

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
            _folderName = folderName;
            _messageId = messageId;
            _mediaType = mediaType;

            AppDelegate.ActiveDownloadCancelationToken = new CancellationTokenSource();
        }

        /// <summary>
        /// Performs an asynchronous Media request
        /// </summary>
        /// <returns></returns>
        public async Task GetMediaAsync()
        {
            var fileName = string.Concat(DateTime.Now.ToString("MMddyyyy_"), _messageId, ".", _mediaType);
            var filePath = Path.Combine(AppDelegate.TempFolderPath, fileName);

            if (File.Exists(filePath))
            {
                FilePath = filePath;
                return;
            }

            if (PhoneCapability.NetworkIsUnreachable)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            IsBusy = true;

            StartWatcher();

            var errorResponse = string.Empty;
            var requestResult = await _service.ExecuteRequest(null, _systemPhoneNumber, _mailboxNumber, _folderName, _messageId, _mediaType, AppDelegate.ActiveDownloadCancelationToken.Token);
            if (requestResult is ErrorResponse)
                errorResponse = ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as GetMediaResponse;
                if (data != null)
                    FilePath = data.FilePath;
            }

            StopWatcher(errorResponse);

            IsBusy = false;
        }
    }
}