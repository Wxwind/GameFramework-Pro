using System;
using System.Buffers;
using System.IO;

namespace GFPro
{
    public class MemoryBuffer : MemoryStream, IBufferWriter<byte>
    {
        private int origin;

        public MemoryBuffer()
        {
        }

        public MemoryBuffer(int capacity) : base(capacity)
        {
        }

        public MemoryBuffer(byte[] buffer) : base(buffer)
        {
        }

        public MemoryBuffer(byte[] buffer, int index, int length) : base(buffer, index, length)
        {
            origin = index;
        }

        public ReadOnlyMemory<byte> WrittenMemory => GetBuffer().AsMemory(origin, (int)Position);

        public ReadOnlySpan<byte> WrittenSpan => GetBuffer().AsSpan(origin, (int)Position);

        public void Advance(int count)
        {
            var newLength = Position + count;
            if (newLength > Length)
            {
                SetLength(newLength);
            }

            Position = newLength;
        }

        public Memory<byte> GetMemory(int sizeHint = 0)
        {
            if (Length - Position < sizeHint)
            {
                SetLength(Position + sizeHint);
            }

            var memory = GetBuffer().AsMemory((int)Position + origin, (int)(Length - Position));
            return memory;
        }

        public Span<byte> GetSpan(int sizeHint = 0)
        {
            if (Length - Position < sizeHint)
            {
                SetLength(Position + sizeHint);
            }

            var span = GetBuffer().AsSpan((int)Position + origin, (int)(Length - Position));
            return span;
        }
    }
}