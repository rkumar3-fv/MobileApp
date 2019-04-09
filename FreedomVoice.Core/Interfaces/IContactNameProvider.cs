using System;

namespace FreedomVoice.Core.ViewModels
{
    public interface IContactNameProvider
    {
        string GetName(string phone);
        void RequestContacts();
        event EventHandler ContactsUpdated;
    }
}