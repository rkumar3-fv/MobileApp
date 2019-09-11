using System;
using System.Linq;
using System.Text;
using Foundation;
using UserNotifications;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Core;
using System.Collections.Generic;
using FreedomVoice.iOS.Core.Utilities.Helpers;
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;
using FreedomVoice.Core.Utils.Interfaces;
using FreedomVoice.Entities.Enums;

namespace FreedomVoice.iOS.NotificationsServiceExtension
{
    [Register("NotificationService")]
    public class NotificationService : UNNotificationServiceExtension
    {
        private Action<UNNotificationContent> ContentHandler;
        private UNMutableNotificationContent BestAttemptContent;
        private readonly ILogger _logger;
        
        protected NotificationService(IntPtr handle) : base(handle)
        {
            FreedomVoice.iOS.Core.iOSCoreConfigurator.RegisterServices();
            ServiceContainer.Register<IPhoneFormatter>(() => new PhoneFormatter());
            _logger = ServiceContainer.Resolve<ILogger>();
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)} init", "");
        }

        public override void DidReceiveNotificationRequest(UNNotificationRequest request, Action<UNNotificationContent> contentHandler)
        {
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", "DidReceiveNotificationRequest");
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Original content: {request.Content}");
            
            // Save handler and cope push content
            ContentHandler = contentHandler;
            BestAttemptContent = (UNMutableNotificationContent)request.Content.MutableCopy();
            var pushNotificationData = PushResponseExtension.CreateFrom(request.Content.UserInfo);

            if (pushNotificationData?.Data == null)
            {
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", "Message data is missing in Push data");
                ContentHandler?.Invoke(BestAttemptContent);
                return;
            }

            switch (pushNotificationData.PushType)
            {
                case PushType.NewMessage: // Handler only this type
                    ProcessNewMessagePushNotification(pushNotificationData.TextMessageReceivedFromNumber());
                    break;

                default: // Don't handler another types
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Push-notification type({pushNotificationData.PushType}) is not supported");
                    ContentHandler?.Invoke(BestAttemptContent);
                    break;
            }
        }
        
        private void ProcessNewMessagePushNotification(string fromPhoneNumber)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fromPhoneNumber))
                {
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", "Collocutor phone is missing in push data");
                    ContentHandler?.Invoke(BestAttemptContent);
                    return;
                }

                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Collocutor phone is {fromPhoneNumber}");

                // Fetch contacts book
                ContactsHelper.GetContactsList(contacts =>
                {
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Contacts from book: {contacts}");

                    // Display debug info about contacts book
                    DebugPrintContracts(contacts);

                    // Try fetch phone number from push
                    var fromPhoneNumberNormalized = NormalizePhoneNumber(fromPhoneNumber);
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Phone from push: {fromPhoneNumberNormalized}");

                    // Find contact from Contact book by phone
                    var matchedContactName = FirstContact(contacts, fromPhoneNumberNormalized)?.DisplayName;
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Contact is found: {matchedContactName}");

                    // Set custom push title
                    BestAttemptContent.Title = string.IsNullOrWhiteSpace(matchedContactName) ? BestAttemptContent.Title : matchedContactName;
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Modified content: {BestAttemptContent}");

                    ContentHandler?.Invoke(BestAttemptContent);
                });
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
                ContentHandler?.Invoke(BestAttemptContent);
            }
        }

        private FVContact FirstContact(IEnumerable<FVContact> contacts, string byPhoneNumber) {
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Try to find contact by {byPhoneNumber}");

            // Linq is not used here, because Linq works very poorly in iOS extensions.
            foreach (var contact in contacts)
                if (contact.Phones != null && contact.Phones.Count() > 0)
                    foreach (var phone in contact.Phones)
                        if (NormalizePhoneNumber(phone.Number) == NormalizePhoneNumber(byPhoneNumber))
                            return contact;

            return null;
        }

        public override void TimeWillExpire()
        {
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", "TimeWillExpire");

            // Called just before the extension will be terminated by the system.
            // Use this as an opportunity to deliver your "best attempt" at modified content, otherwise the original push payload will be used.

            ContentHandler?.Invoke(BestAttemptContent);
        }
        
        private void DebugPrintContracts(IEnumerable<FVContact> contacts)
        {
            var contactsDebugList = new StringBuilder();
            foreach (var c in contacts)
            {
                var phones = new StringBuilder();
                foreach (var phone in c.Phones)
                    phones.AppendLine(phone.Label + ": " + phone.Number);

                contactsDebugList.AppendLine($"Contact: {c.DisplayName}" +
                                             $"Phones Count: {c.Phones.Count()} \n" +
                                             $"Phones: {phones.ToString()}"
                );
            }

            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Contacts.ContactList: {contactsDebugList.ToString()}");
        }

        private string NormalizePhoneNumber(string fromPhoneNumber)
        {
            var result = "";
            var pattern = "0123456789";
            foreach (var c in fromPhoneNumber)
                if (pattern.Contains(c))
                    result.Append(c);

            return result;
        }
    }
}
