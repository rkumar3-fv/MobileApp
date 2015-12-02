using System;
using System.Diagnostics;
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
        private readonly IMediaService _service;

        private readonly string _systemPhoneNumber;
        private readonly int _mailboxNumber;
        private readonly string _folderName;
        private readonly string _messageId;
        private readonly MediaType _mediaType;

        public string FilePath { get; private set; }

        /// <summary>
        /// Constructor, requires an IService
        /// </summary>
        public MediaViewModel(string systemPhoneNumber, int mailboxNumber, string folderName, string messageId, MediaType mediaType)
        {
            _service = ServiceContainer.Resolve<IMediaService>();

            ProgressControl = ProgressControlType.ProgressBar;

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
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            IsBusy = true;

            var watcher = Stopwatch.StartNew();

            await RenewCookieIfNeeded();

            ProgressBar.Progress = 0;

            var progressReporter = new Progress<DownloadBytesProgress>();
            progressReporter.ProgressChanged += (s, args) => ProgressBar.Progress = args.PercentComplete;

            var tokenSource = new CancellationTokenSource();

            CancelDownloadButton.TouchUpInside += (sender, args) => tokenSource.Cancel();

            var requestResult = await _service.ExecuteRequest(progressReporter, _systemPhoneNumber, _mailboxNumber, _folderName, _messageId, _mediaType, tokenSource.Token);
            watcher.Stop();
            Log.ReportTime(Log.EventCategory.FileLoading, "GetMedia", "", watcher.ElapsedMilliseconds);

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