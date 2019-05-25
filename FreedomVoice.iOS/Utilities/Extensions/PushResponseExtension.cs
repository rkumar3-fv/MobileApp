using System;
using System.Linq;
using Foundation;
using FreedomVoice.Core.Utils;
using FreedomVoice.Entities.Response;
using Newtonsoft.Json;

namespace FreedomVoice.iOS
{
    public static class PushResponseExtension
    {
        public static string TextMessageReceivedFromNumber(this PushResponse<Conversation> self)
        {
            return self?.Data?.Messages.FirstOrDefault()?.From?.PhoneNumber;
        }

        public static string TextMessageReceivedToNumber(this PushResponse<Conversation> self)
        {
            return self?.Data?.Messages.FirstOrDefault()?.To?.PhoneNumber;
        }

        public static PushResponse<Conversation> CreateFromFromJson(NSDictionary userInfo)
        {
            var logger = ServiceContainer.Resolve<Core.ILogger>();
            logger.Debug(nameof(PushResponseExtension), nameof(CreateFromFromJson), $"Try parse PushResponse<Conversation> from userInfo: {userInfo}");

            try
            {
                var jsonDataObject = userInfo["data"];
                var ser = NSJsonSerialization.Serialize(jsonDataObject, NSJsonWritingOptions.PrettyPrinted,
                    out var error);

                if (error != null)
                {
                    logger.Debug(nameof(PushResponseExtension), nameof(CreateFromFromJson), $"NSJsonSerialization error: {error}");
                    return null;
                }

                var jsonString = NSString.FromData(ser, NSStringEncoding.UTF8);
                var pushResponse = JsonConvert.DeserializeObject<PushResponse<Conversation>>(jsonString, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

                return pushResponse;
            }
            catch (Exception ex)
            {
                logger.Debug(nameof(PushResponseExtension), nameof(CreateFromFromJson),
                    $"PushResponse<Conversation> has been failed: {ex}");
                return null;
            }
        }
    }
}
