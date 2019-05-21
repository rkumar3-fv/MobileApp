using System;
using System.Json;
using Foundation;
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
            try
            {
                //TODO Refach
                Console.WriteLine($"USER INFO: {userInfo.ToString()}");

                var jsonData = NSJsonSerialization.Serialize(userInfo, NSJsonWritingOptions.PrettyPrinted, out var error);
                Console.WriteLine($"JSON DATA ({error}) INFO: {jsonData}");

                if (error != null)
                {
                    Console.WriteLine($"{error}");
                    return null;
                }

                var jsonString = NSString.FromData(jsonData, NSStringEncoding.UTF8);
                Console.WriteLine($"JSON STRING {jsonString}");

                var jsonValue = JsonValue.Parse(jsonString);

                if (!jsonValue.ContainsKey("Data"))
                    return null;

                var dataMainJsonValue = JsonValue.Parse(jsonValue["Data"].ToString());
                var dataJsonValue = JsonValue.Parse(dataMainJsonValue["Data"].ToString());
                var collocutorPhoneJsonValue = JsonValue.Parse(dataJsonValue["CollocutorPhone"].ToString());
                var currentPhoneeJsonValue = JsonValue.Parse(dataJsonValue["CurrentPhone"].ToString());

                long pushTypeLongValue = dataMainJsonValue["PushType"];

                var pushResponse = new PushResponse<Conversation>
                {
                    PushType = (PushType)pushTypeLongValue,
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
                            PhoneNumber = currentPhoneeJsonValue["PhoneNumber"],
                            Id = currentPhoneeJsonValue["Id"]
                        }
                    }
                };

                Console.WriteLine($"USER CAST: {pushResponse}");
                return pushResponse;
            }
            catch (Exception e)
            {
                Console.WriteLine($"USER DAT ERROR: {e}");
                return null;
            }
        }
    }
}
