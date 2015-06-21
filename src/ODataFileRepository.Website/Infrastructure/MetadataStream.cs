using System;
using System.IO;

namespace ODataFileRepository.Website.Infrastructure
{
    public class MetadataStream<TMetadata> : Stream where TMetadata: class
    {
        private readonly Stream _stream;

        private readonly TMetadata _metadata;

        public MetadataStream(Stream stream, TMetadata metadata)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            _stream = stream;
            _metadata = metadata;
        }

        public TMetadata Metadata { get { return _metadata; } }

        public override bool CanRead
        {
            get
            {
                return _stream.CanRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return _stream.CanSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return _stream.CanWrite;
            }
        }

        public override long Length
        {
            get
            {
                return _stream.Length;
            }
        }

        public override long Position
        {
            get
            {
                return _stream.Position;
            }

            set
            {
                _stream.Position = value;
            }
        }

        public override void Flush()
        {
            _stream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _stream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _stream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _stream.Write(buffer, offset, count);
        }
    }
}