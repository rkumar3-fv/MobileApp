using System;
using System.Collections.Generic;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.Entities.Request.Texting
{
    public class PushRequest
    {
        private DeviceType _type;

        public DeviceType Type
        {
            get => _type;
            set
            {
                if (!Enum.IsDefined(typeof(DeviceType), value))
                    throw new Exception("Provided incorrect device type");

                _type = value;
            }
        }

        public string SystemPhone { get; set; }

        public string ApplicationNumber { get; set; }

        public string Token { get; set; }

        public IEnumerable<string> Phones { get; set; }
    }
}
