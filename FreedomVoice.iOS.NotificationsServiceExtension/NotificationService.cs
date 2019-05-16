using System;
using System.Linq;
using System.Text;
using Foundation;
using FreedomVoice.iOS.NotificationsServiceExtension.Models;
using UserNotifications;
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;
using FreedomVoice.Core.Utils;
using FreedomVoice.Core.ViewModels;

namespace FreedomVoice.iOS.NotificationsServiceExtension
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        Action<UNNotificationContent> ContentHandler { get; set; }
        UNMutableNotificationContent BestAttemptContent { get; set; }
        
        
        protected NotificationService(IntPtr handle) : base(handle)
        {
            FreedomVoice.iOS.Core.iOSCoreConfigurator.RegisterServices();
            
            Console.WriteLine($"[{this.GetType()}] DidReceiveNotificationRequest");
            // Note: this .ctor should not contain any initialization logic.
        }

        public override async void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            Console.WriteLine($"[{this.GetType()}] DidReceiveNotificationRequest");
            Console.WriteLine($"[{this.GetType()}] Original content: {request.Content}");
            
            // Save handler and cope push content
            ContentHandler = contentHandler;
            BestAttemptContent = (UNMutableNotificationContent)request.Content.MutableCopy();
            var contactNameProvider = ServiceContainer.Resolve<IContactNameProvider>();
            var pushNotificationData = new PushNotification(request.Content);

            if (pushNotificationData?.data?.message == null)
            {
                Console.WriteLine($"[{this.GetType()}] Message data is missing in Push data");
                ContentHandler?.Invoke(BestAttemptContent);
                return;

            }

            Console.WriteLine($"[{this.GetType()}] User data: \n{pushNotificationData}");            

            // Fetch contacts book
            await ContactsHelper.GetContactsListAsync();
            
            // Display debug info about contacts book
            DebugPrintContracts();
            
            // Try fetch phone number from push
            var phoneFromPush = pushNotificationData.data?.message?.fromPhoneNumber;
            phoneFromPush = contactNameProvider.GetClearPhoneNumber(phoneFromPush);
            Console.WriteLine($"[{this.GetType()}] Phone from push: {phoneFromPush}");

            // Find contact from Contact book by phone
            var matchedContactName = contactNameProvider.GetNameOrNull(phoneFromPush);
            Console.WriteLine($"[{this.GetType()}] Contact is found: {matchedContactName}");

            // Set custom push title
            BestAttemptContent.Title = string.IsNullOrWhiteSpace(matchedContactName) ? request.Content?.Title : matchedContactName;
            Console.WriteLine($"[{this.GetType()}] Modified content: {BestAttemptContent}");

            ContentHandler?.Invoke(BestAttemptContent);
        }

        public override void TimeWillExpire()
        {
            Console.WriteLine($"[{this.GetType()}] TimeWillExpire");

            // Called just before the extension will be terminated by the system.
            // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.

            ContentHandler?.Invoke(BestAttemptContent);
        }
        
        private void DebugPrintContracts()
        {
            var contactsDebugList = new StringBuilder();
            foreach (var c in ContactsHelper.ContactList)
            {
                var phones = new StringBuilder();
                foreach (var phone in c.Phones)
                {
                    phones.AppendLine(phone.Label + ": " + phone.Number);
                }

                contactsDebugList.AppendLine($"Contact: {c.DisplayName}" +
                                             $"Phones Count: {c.Phones.Count()} \n" +
                                             $"Phones: {phones.ToString()}"
                );
            }
            Console.WriteLine($"[{this.GetType()}] Contacts.ContactList: {contactsDebugList.ToString()}");
        }
    }
}
