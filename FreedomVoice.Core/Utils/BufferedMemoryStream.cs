using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;

namespace FreedomVoice.Core.Utils
{
    public class BufferedMemoryStream : MemoryStream
    {
        private readonly ManualResetEvent _dataReady;
        private readonly ConcurrentQueue<byte[]> _buffers;

        public BufferedMemoryStream()
        {
            _dataReady = new ManualResetEvent(false);
            _buffers = new ConcurrentQueue<byte[]>();
        }

        public bool DataAvailable => !_buffers.IsEmpty;

        public override void Write(byte[] buffer, int offset, int count)
        {
            _buffers.Enqueue(buffer);
            _dataReady.Set();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            _dataReady.WaitOne();
            byte[] lBuffer;
            if (!_buffers.TryDequeue(out lBuffer))
            {
                _dataReady.Reset();
                return -1;
            }
            if (!DataAvailable)
                _dataReady.Reset();
            Array.Copy(lBuffer, buffer, lBuffer.Length);
            return lBuffer.Length;
        }
    }
}