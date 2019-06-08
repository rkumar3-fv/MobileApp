using FreedomVoice.Core.Entities.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace FreedomVoice.Core.Entities.Texting
{
    public class BaseResponse
    {
        public ErrorCodes ResponseCode { get; set; }
        public string Message { get; set; }
    }
}
