﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Entities;
using FreedomVoice.iOS.Services;
using FreedomVoice.iOS.Services.Responses;
using FreedomVoice.iOS.Utilities;
using FreedomVoice.iOS.Utilities.Helpers;

namespace FreedomVoice.iOS.ViewModels
{
    public class FoldersViewModel : BaseViewModel
    {
        private readonly IFoldersService _service;

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
        public async Task GetFoldersListAsync(bool silent = false)
        {
            if (PhoneCapability.NetworkIsUnreachable && !silent)
            {
                Appearance.ShowOkAlertWithMessage(Appearance.AlertMessageType.NetworkUnreachable);
                return;
            }

            if (!silent)
                IsBusy = true;

            await RenewCookieIfNeeded();

            var requestResult = await _service.ExecuteRequest(_systemPhoneNumber, _mailboxNumber);
            if (requestResult is ErrorResponse)
            {
                if (!silent)
                    ProceedErrorResponse(requestResult);
            }
            else
            {
                var data = requestResult as FoldersWithCountResponse;
                if (data != null)
                    FoldersList = data.FoldersWithCount;
            }

            if (!silent)
                IsBusy = false;
        }
    }
}