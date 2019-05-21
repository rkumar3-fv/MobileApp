using System;
using System.Collections.Generic;

namespace FreedomVoice.Core.ViewModels
{
    public interface IContactNameProvider
    {
        string GetName(string phone);
        
        string GetNameOrNull(string phone);

        string GetFormattedPhoneNumber(string phoneNumber);
        string GetClearPhoneNumber(string formattedPhoneNumber);
        
        List<string> SearchNumbers(string query);
        
        void RequestContacts();
        event EventHandler ContactsUpdated;
    }
}