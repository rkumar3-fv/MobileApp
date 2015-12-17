using System.Collections.Generic;
using FreedomVoice.iOS.Entities;
using Newtonsoft.Json;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Recents
    {
        public static void StoreRecentsToCache()
        {
            UserDefault.RecentsCache = AppDelegate.RecentsList.Count != 0 ? JsonConvert.SerializeObject(AppDelegate.RecentsList) : string.Empty;
        }

        public static void RestoreRecentsFromCache()
        {
            var recentsCache = UserDefault.RecentsCache;

            AppDelegate.RecentsList = string.IsNullOrEmpty(recentsCache) ? new List<Recent>() : JsonConvert.DeserializeObject<List<Recent>>(recentsCache);
        }

        public static void ClearRecents()
        {
            AppDelegate.RecentsList = new List<Recent>();
            UserDefault.RecentsCache = string.Empty;
        }
    }
}