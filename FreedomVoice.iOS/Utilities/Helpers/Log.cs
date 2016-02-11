using Google.Analytics;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Log
    {
        private const string RequestKey = "API_REQUEST";
        private const string LoadingKey = "LOADING_FILE";
        private const string ActionKey = "LONG_ACTION";
        private const string OtherKey = "OTHER";

        public enum EventCategory
        {
            Request,
            FileLoading,
            LongAction
        }

        public static void ReportTime(EventCategory eventCategory, string name, string result, long time)
        {
            string category;
            switch (eventCategory)
            {
                case EventCategory.Request:
                    category = RequestKey;
                    break;
                case EventCategory.FileLoading:
                    category = LoadingKey;
                    break;
                case EventCategory.LongAction:
                    category = ActionKey;
                    break;
                default:
                    category = OtherKey;
                    break;
            }

            Gai.SharedInstance.DefaultTracker.Send(DictionaryBuilder.CreateTiming(category, time, name, result).Build());
        }

        public static void ReportEvent(string name, string result)
        {
            Gai.SharedInstance.DefaultTracker.Send(DictionaryBuilder.CreateEvent(OtherKey, name, result, 1).Build());
        }
    }
}