using System;
using System.Collections.Generic;
using System.Linq;
using FreedomVoice.iOS.Entities;
using Newtonsoft.Json;

namespace FreedomVoice.iOS.Utilities.Helpers
{
    public static class Recents
    {
        private static List<Recent> RecentsList { get; set; }
        public static int RecentsCount => RecentsList.Count;

        public static void StoreRecentsToCache()
        {
            UserDefault.RecentsCache = RecentsCount != 0 ? JsonConvert.SerializeObject(RecentsList) : string.Empty;
        }

        public static void RestoreRecentsFromCache()
        {
            var recentsCache = UserDefault.RecentsCache;

            RecentsList = string.IsNullOrEmpty(recentsCache) ? new List<Recent>() : JsonConvert.DeserializeObject<List<Recent>>(recentsCache);
        }

        public static Recent GetLastRecent()
        {
            return GetRecentsOrdered().First();
        }

        public static void RemoveRecent(Recent recent)
        {
            RecentsList.Remove(recent);
        }

        public static List<Recent> GetRecentsOrdered()
        {
            return RecentsList.OrderByDescending(r => r.DialDate).ToList();
        }

        public static void AddRecent(string phoneNumber, string title = "", string contactId = "")
        {
            var existingRecent = RecentsList.FirstOrDefault(r => Core.Utilities.Helpers.Contacts.NormalizePhoneNumber(r.PhoneNumber) == Core.Utilities.Helpers.Contacts.NormalizePhoneNumber(phoneNumber));
            if (existingRecent == null)
                RecentsList.Add(new Recent(title, phoneNumber, DateTime.Now, contactId));
            else
            {
                existingRecent.DialDate = DateTime.Now;
                existingRecent.CallsQuantity++;
            }
        }

        public static void ClearRecents()
        {
            RecentsList.Clear();
            UserDefault.RecentsCache = string.Empty;
        }
    }
}