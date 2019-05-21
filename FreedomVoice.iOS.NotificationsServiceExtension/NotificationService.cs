using System;
using System.Linq;
using System.Text;
using Foundation;
using UserNotifications;
using FreedomVoice.Core.Utils;
using FreedomVoice.iOS.Core;
using System.Collections.Generic;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using FreedomVoice.iOS.Core.Utilities.Helpers;
using FreedomVoice.iOS.Core.Utilities.Extensions;
using ContactsHelper = FreedomVoice.iOS.Core.Utilities.Helpers.Contacts;

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
                    ProcessNewMessagePushNotification(pushNotificationData);
                    break;

                default: // Don't handler another types
                    _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Push-notification type({pushNotificationData.PushType}) is not supported");
                    ContentHandler?.Invoke(BestAttemptContent);
                    break;
            }
        }
        
        private void ProcessNewMessagePushNotification(PushResponse<Conversation> pushNotificationData)
        {
            if (string.IsNullOrWhiteSpace(pushNotificationData.Data.CollocutorPhone?.PhoneNumber))
            {
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", "Collocutor phone is missing in push data");
                ContentHandler?.Invoke(BestAttemptContent);
                return;
            }
            
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"User data: \n{pushNotificationData}");

            // Fetch contacts book
            ContactsHelper.GetContactsList(contacts =>
            {
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Contacts from book: {contacts.Count()}");

                // Display debug info about contacts book
                DebugPrintContracts(contacts);

                // Try fetch phone number from push
                var phoneFromPush = ContactsHelper.NormalizePhoneNumber(pushNotificationData.Data.CollocutorPhone.PhoneNumber);
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Phone from push: {phoneFromPush}");

                // Find contact from Contact book by phone
                var matchedContactName = FirstContact(contacts, phoneFromPush)?.DisplayName;
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Contact is found: {matchedContactName}");

                // Set custom push title
                BestAttemptContent.Title = string.IsNullOrWhiteSpace(matchedContactName) ? BestAttemptContent.Title : matchedContactName;
                _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Modified content: {BestAttemptContent}");

                ContentHandler?.Invoke(BestAttemptContent);
            });
        }

        private FVContact FirstContact(IEnumerable<FVContact> contacts, string byPhoneNumber) {
            _logger.Debug($"{nameof(NotificationService)}", $"{nameof(NotificationService)}", $"Try to find contact by {byPhoneNumber}");

            // Linq is not used here, because Linq works very poorly in iOS extensions.
            foreach (var contact in contacts)
                if (contact.Phones != null && contact.Phones.Count() > 0)
                    foreach (var phone in contact.Phones)
                        if (ContactsHelper.NormalizePhoneNumber(phone.Number) == ContactsHelper.NormalizePhoneNumber(byPhoneNumber))
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
    }
}
