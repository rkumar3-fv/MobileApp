using System.Collections.Generic;
using FreedomVoice.Core.Entities;
using FreedomVoice.iOS.Entities;

namespace FreedomVoice.iOS.Services.Responses
{
    public class FoldersWithCountResponse : BaseResponse
    {
        public List<FolderWithCount> FoldersWithCount { get; }

        /// <summary>
        /// Response init for FoldersService
        /// </summary>
        /// <param name="foldersWithCount">Folders With Count</param>
        public FoldersWithCountResponse(IEnumerable<MessageFolderWithCounts> foldersWithCount)
        {
            FoldersWithCount = new List<FolderWithCount>();

            foreach (var folder in foldersWithCount)
                FoldersWithCount.Add(new FolderWithCount(folder));
        }
    }
}