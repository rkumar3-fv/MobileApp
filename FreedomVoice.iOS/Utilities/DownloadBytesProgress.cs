namespace FreedomVoice.iOS.Utilities
{
    public class DownloadBytesProgress
    {
        public DownloadBytesProgress(long bytesReceived, long totalBytes)
        {
            BytesReceived = bytesReceived;
            TotalBytes = totalBytes;
        }

        private long TotalBytes { get; }

        private long BytesReceived { get; }

        public float PercentComplete => (float)BytesReceived / TotalBytes;

        public bool IsFinished => BytesReceived == TotalBytes;
    }
}