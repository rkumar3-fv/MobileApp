using System;
using System.Collections.Generic;
using System.Json;
using System.Linq;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Response;
using FreedomVoice.iOS.Core;

namespace FreedomVoice.iOS.NotificationsServiceExtension
{
    internal static class PushResponseExtension
    {
        public static string TextMessageReceivedFromNumber(this PushResponse<Conversation> self)
        {
            return self?.Data?.Messages.First()?.From?.PhoneNumber;
        }
        
        public static PushResponse<Conversation> CreateFrom(NSDictionary userInfo)
        {
            var logger = ServiceContainer.Resolve<ILogger>();
            logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"Try parse PushResponse<Conversation> from userInfo: {userInfo}");

            try
            {
                var jsonData = NSJsonSerialization.Serialize(userInfo, NSJsonWritingOptions.PrettyPrinted, out var error);
                logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"Parsed json data: {jsonData}");
            
                if (error != null)
                {
                    logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"PushResponse<Conversation> has been parsed with error: {error}");
                    return null;
                }

                var jsonString = NSString.FromData(jsonData, NSStringEncoding.UTF8);
                logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"Parsed json string: {jsonString}");

                var jsonValue = JsonValue.Parse(jsonString);

                if (!jsonValue.ContainsKey("data"))
                    return null;
                
                var dataMainJsonValue = JsonValue.Parse(jsonValue["data"].ToString());
                var dataJsonValue = JsonValue.Parse(dataMainJsonValue["data"].ToString());
                long pushTypeLongValue = dataMainJsonValue["pushType"];

                var pushResponse = new PushResponse<Conversation>
                {
                    PushType = (PushType)pushTypeLongValue,
                    Data = new Conversation
                    {
                        Id = dataJsonValue["id"],
                        Messages = new List<Message>
                        {
                            new Message
                            {
                               From = new Phone
                               {
                                   Id = dataJsonValue["messages"][0]["from"]["id"],
                                   PhoneNumber = dataJsonValue["messages"][0]["from"]["phoneNumber"]
                               }
                            }
                        }
                    }
                };

                logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"PushResponse<Conversation> has been parsed: {pushResponse}");
                return pushResponse;
            }
            catch (Exception ex)
            {
                logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"PushResponse<Conversation> has been failed: {ex}");
                return null;
            }
        }
    }
}
