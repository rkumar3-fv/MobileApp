using System;
using System.IO;
using System.Linq;
using System.Text;
using Foundation;
using FreedomVoice.iOS.NotificationsServiceExtension.Models;
using FreedomVoice.iOS.NotificationsServiceExtension.Utils;
using Newtonsoft.Json;
using UserNotifications;

namespace FreedomVoice.iOS.NotificationsServiceExtension
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        Action<UNNotificationContent> ContentHandler { get; set; }
        UNMutableNotificationContent BestAttemptContent { get; set; }
        
        protected NotificationService(IntPtr handle) : base(handle)
        {
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
           
            var pushNotificationData = new PushNotification(request.Content);
            Console.WriteLine($"[{this.GetType()}] User data: \n{pushNotificationData}");
            
            // Fetch contacts book
            await Utils.Contacts.GetContactsListAsync();

            // Display debug info about contacts book
            DebugPrintContracts();

            // Try fetch phone number from push
            var phoneFromPush = pushNotificationData.data?.message?.fromPhoneNumber;
            Console.WriteLine($"[{this.GetType()}] phone: {phoneFromPush}");

            // Find contact from Contact book by phone
            var matchedContact = Utils.Contacts.ContactList.FirstOrDefault(contact =>
                contact.Phones.FirstOrDefault(phone => DataFormatUtils.NormalizePhone(phone.Number) == DataFormatUtils.NormalizePhone(phoneFromPush))?.Label != null
            );
            Console.WriteLine($"[{this.GetType()}] Contact is found: {matchedContact}");

            // Set custom push title
            BestAttemptContent.Title = matchedContact?.DisplayName ?? request.Content?.Title;
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
            foreach (var c in Utils.Contacts.ContactList)
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
