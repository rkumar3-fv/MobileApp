namespace FreedomVoice.Core.Entities.EventArgs
{
    using Enums;

    public class DownloadStatusArgs
    {
        public int Progress { get; set; }

        public DownloadStatus Status { get; set; }
    }
}