using System;

namespace FreedomVoice.Core.ViewModels
{
    public interface IContactNameProvider
    {
        string GetName(string phone);
        
        string GetNameOrNull(string phone);

        string GetFormattedPhoneNumber(string phoneNumber);
        string GetClearPhoneNumber(string formattedPhoneNumber);
        
        void RequestContacts();
        event EventHandler ContactsUpdated;
    }
}