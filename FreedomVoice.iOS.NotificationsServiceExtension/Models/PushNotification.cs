using System;
using Foundation;
using UserNotifications;

namespace FreedomVoice.iOS.NotificationsServiceExtension.Models
{
	public class PushNotification
	{
		public Data data { get; }

		public PushNotification(UNNotificationContent content)
		{
			Console.WriteLine(content.UserInfo);

			if (content.UserInfo.ContainsKey((NSString)PushNotificationKeys.data.GetName()))
				this.data = new Data();
			else
				return;

			var data = content.UserInfo[PushNotificationKeys.data.GetName()] as NSDictionary;
			Console.WriteLine($"[{this.GetType()}] Data is Created");

			if (data.ContainsKey((NSString)PushNotificationKeys.message.GetName()))
				this.data.message = new Message();
			else
				return;
                
			var message = data[PushNotificationKeys.message.GetName()] as NSDictionary;
			Console.WriteLine($"[{this.GetType()}] Message is Created");

			if (message.ContainsKey((NSString) PushNotificationKeys.conversationId.GetName()))
			{
				var conversationIdStringValue = message[PushNotificationKeys.conversationId.GetName()]?.ToString();
				if (long.TryParse(conversationIdStringValue, out var conversationId))
					this.data.message.conversationId = conversationId;

				Console.WriteLine($"[{this.GetType()}] conversationId is created ({conversationIdStringValue}: {this.data.message.conversationId})");
			}
			else
			{
				Console.WriteLine($"[{this.GetType()}] conversationId is absent");
			}

			if (message.ContainsKey((NSString) PushNotificationKeys.messageId.GetName()))
			{
				var messageIdStringValue = message[PushNotificationKeys.messageId.GetName()]?.ToString();
				if (long.TryParse(messageIdStringValue, out var messageId))
					this.data.message.messageId = messageId;

				Console.WriteLine($"[{this.GetType()}] messageId is created ({messageIdStringValue}: {this.data.message.messageId})");
			}
			else
			{
				Console.WriteLine($"[{this.GetType()}] messageId is absent");
			}


			if (message.ContainsKey((NSString) PushNotificationKeys.fromPhoneNumber.GetName()))
			{
				var fromPhoneNumberStringValue = message[PushNotificationKeys.fromPhoneNumber.GetName()]?.ToString();
				this.data.message.fromPhoneNumber = fromPhoneNumberStringValue;

				Console.WriteLine($"[{this.GetType()}] fromPhoneNumber is created ({fromPhoneNumberStringValue}: {this.data.message.fromPhoneNumber})");
			}
			else
			{
				Console.WriteLine($"[{this.GetType()}] fromPhoneNumber is absent");
			}

			if (message.ContainsKey((NSString) PushNotificationKeys.toPhoneNumber.GetName()))
			{
				var fromPhoneNumberStringValue = message[PushNotificationKeys.toPhoneNumber.GetName()]?.ToString();
				this.data.message.toPhoneNumber = fromPhoneNumberStringValue;

				Console.WriteLine($"[{this.GetType()}] toPhoneNumber is created ({fromPhoneNumberStringValue}: {this.data.message.toPhoneNumber})");
			}
			else
			{
				Console.WriteLine($"[{this.GetType()}] toPhoneNumber is absent");
			}
		}

		public override string ToString()
		{
			return $"{this.GetType()}\n" +
			       $"Data: {data}\n" +
			       $"Data.message: {data?.message}\n" +
			       $"Data.message.messageId: {data?.message?.messageId}\n" +
			       $"Data.message.conversationId: {data?.message?.conversationId}\n" +
			       $"Data.message.toPhoneNumber: {data?.message?.toPhoneNumber}\n" +
			       $"Data.message.fromPhoneNumber: {data?.message?.fromPhoneNumber}";
		}
	}
}