using System;
using System.Json;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities;
using FreedomVoice.Entities.Enums;
using FreedomVoice.Entities.Request;
using FreedomVoice.Entities.Response;
using UserNotifications;

namespace FreedomVoice.iOS.Core.Utilities.Extensions
{
    public static class PushResponseExtension
    {
        public static PushResponse<Conversation> CreateFrom(NSDictionary userInfo)
        {
            var logger = ServiceContainer.Resolve<ILogger>();
            logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"Try parse PushResponse<Conversation> from userInfo: {userInfo}");
            
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

            if (!jsonValue.ContainsKey("Data"))
                return null;

            var dataMainJsonValue = JsonValue.Parse(jsonValue["Data"].ToString());
            var dataJsonValue = JsonValue.Parse(dataMainJsonValue["Data"].ToString());
            var collocutorPhoneJsonValue = JsonValue.Parse(dataJsonValue["CollocutorPhone"].ToString());
            var currentPhoneJsonValue = JsonValue.Parse(dataJsonValue["CurrentPhone"].ToString());
            long pushTypeLongValue = dataMainJsonValue["PushType"];

            var pushResponse = new PushResponse<Conversation>
            {
                PushType = (PushType) pushTypeLongValue,
                Data = new Conversation
                {
                    Id = dataJsonValue["Id"],
                    CollocutorPhone = new Phone
                    {
                        PhoneNumber = collocutorPhoneJsonValue["PhoneNumber"],
                        Id = collocutorPhoneJsonValue["Id"]
                    },
                    CurrentPhone = new Phone
                    {
                        PhoneNumber = currentPhoneJsonValue["PhoneNumber"],
                        Id = currentPhoneJsonValue["Id"]
                    }
                }
            };

            logger.Debug(nameof(PushResponseExtension), nameof(CreateFrom), $"PushResponse<Conversation> has been parsed: {pushResponse}");
            return pushResponse;
        }
    }
}
