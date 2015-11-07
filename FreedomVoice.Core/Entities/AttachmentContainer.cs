using FreedomVoice.Core.Utils;

namespace FreedomVoice.Core.Entities
{
    public class AttachmentContainer
    {
        public long Size { get; }

        public BufferedMemoryStream BufferedStream { get; }

        public AttachmentContainer(long size, BufferedMemoryStream bufferedStream)
        {
            Size = size;
            BufferedStream = bufferedStream;
        }
    }
}
