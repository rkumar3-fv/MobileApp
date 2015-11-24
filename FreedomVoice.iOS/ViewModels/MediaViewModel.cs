using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FreedomVoice.Core.Entities.Enums;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;
using UIKit;

namespace FreedomVoice.iOS.ViewModels
{
    public class MediaViewModel : BaseViewModel
    {
        private readonly IMediaService _service;
        private readonly UIViewController _viewController;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _folderName;
        private readonly string _messageId;
        private readonly MediaType _mediaType;

        public string FilePath { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MediaViewModel(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType, UIViewController viewController)
        {
            _service = ServiceContainer.Resolve<IMediaService>();

            ProgressControl = ProgressControlType.ProgressBar;

            ViewController = viewController;
            _viewController = viewController;

            _systemPhoneNumber = systemPhoneNumber;
            _mailboxNumber = mailboxNumber;
            _folderName = folderName;
            _messageId = messageId;
            _mediaType = mediaType;
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
                Appearance.ShowOkAlertWithMessage(_viewController, Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            IsBusy = true;

            await RenewCookieIfNeeded();

            ProgressBar.Progress = 0;

            var progressReporter = new Progress<DownloadBytesProgress>();
            progressReporter.ProgressChanged += (s, args) => ProgressBar.Progress = args.PercentComplete;

            var tokenSource = new CancellationTokenSource();

            CancelDownloadButton.TouchUpInside += (sender, args) => tokenSource.Cancel();

            var requestResult = await _service.ExecuteRequest(progressReporter, _systemPhoneNumber, _mailboxNumber, _folderName, _messageId, _mediaType, tokenSource.Token);
            if (requestResult is ErrorResponse)
                ProceedErrorResponse(requestResult);
            else
            {
                var data = requestResult as GetMediaResponse;
                if (data != null)
                    FilePath = data.FilePath;
            }

            IsBusy = false;
        }
    }
}