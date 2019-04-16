using System;
using System.IO;

namespace FreedomVoice.iOS.Data
{
    public class IosDbPath
    {
        public static string GetDatabasePath(string sqliteFilename)
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", sqliteFilename);
        }
    }
}