﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FreedomVoice.Core;
using FreedomVoice.iOS.Services.Responses;

namespace FreedomVoice.iOS.Services.Implementations
{
    public class MessageOperationsService : BaseService, IMessageOperationsService
    {
        public async Task<BaseResponse> ExecuteMoveRequest(string systemNumber, int mailboxNumber, string destinationFolder, List<string> messageIds)
        {
            var asyncRes = await ApiHelper.MoveMessages(systemNumber, mailboxNumber, destinationFolder, messageIds);
            var errorResponse = CheckErrorResponse(asyncRes, false);
            if (errorResponse != null)
                return errorResponse;

            return new EmptyResponse();
        }

        public async Task<BaseResponse> ExecuteDeleteRequest(string systemNumber, int mailboxNumber, List<string> messageIds)
        {
            var asyncRes = await ApiHelper.DeleteMessages(systemNumber, mailboxNumber, messageIds);
            var errorResponse = CheckErrorResponse(asyncRes, false);
            if (errorResponse != null)
                return errorResponse;

            return new EmptyResponse();
        }
    }
}