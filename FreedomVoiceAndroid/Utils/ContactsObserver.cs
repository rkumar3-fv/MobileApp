using System;
using Android.Database;

namespace com.FreedomVoice.MobileApp.Android.Utils
{
    public class ContactsObserver : ContentObserver
    {
        public event EventHandler<bool> ContactsChangingEvent; 

        public ContactsObserver() : base(null)
        {}

        public override bool DeliverSelfNotifications()
        {
            return true;
        }

        public override void OnChange(bool selfChange)
        {
            base.OnChange(selfChange);
            ContactsChangingEvent?.Invoke(this, selfChange);
        }
    }
}