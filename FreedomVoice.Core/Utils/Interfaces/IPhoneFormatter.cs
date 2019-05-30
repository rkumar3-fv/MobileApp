using System;
namespace FreedomVoice.Core.Utils.Interfaces
{
    public interface IPhoneFormatter
    {
        string Format(string phone);
        string Parse(string phone);
    }
}
