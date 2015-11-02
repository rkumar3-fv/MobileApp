using System;
using System.IO;
using FreedomVoice.Core.Entities.Enums;

namespace FreedomVoice.iOS.Services.Responses
{
    public class MediaResponse : BaseResponse
    {
        public string FilePath { get; }

        /// <summary>
        /// Response init for MediaService
        /// </summary>
        /// <param name="media">File stream</param>
        /// <param name="mediaType">File type</param>
        public MediaResponse(Stream media, MediaType mediaType)
        {
            using (var ms = new MemoryStream())
            {
                media.CopyTo(ms);

                var bytes = ms.ToArray();

                var tmpFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "tmp");
                var fileName = string.Concat(DateTime.Now.ToString("MMddyyyy_hhmmss"), ".", mediaType);

                var filePath = Path.Combine(tmpFolderPath, fileName);

                using (var file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    ms.Read(bytes, 0, (int)ms.Length);
                    file.Write(bytes, 0, bytes.Length);
                    ms.Close();
                }

                FilePath = filePath;
            }
        }
    }
}