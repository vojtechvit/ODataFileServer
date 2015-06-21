using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ODataFileRepository.Website.Infrastructure
{
    public class JoinedStream : Stream
    {
        private readonly Stream[] _streams;

        private readonly Lazy<long> _length;

        private int _currentStreamIndex = 0;

        private long _currentPosition = 0;

        public JoinedStream(IEnumerable<Stream> streams)
        {
            if (streams == null)
            {
                throw new ArgumentNullException("streams");
            }

            _streams = streams.ToArray();

            if (_streams.Length == 0)
            {
                throw new ArgumentOutOfRangeException("streams", "At least one stream must be provided");
            }

            if (_streams.Any(stream => !stream.CanRead))
            {
                throw new ArgumentException("At least one stream is not readable.", "streams");
            }

            _length = new Lazy<long>(() => _streams.Sum(s => s.Length));
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get
            {
                return _length.Value;
            }
        }

        public override long Position
        {
            get { return _currentPosition; }

            set
            {
                throw new InvalidOperationException("Seeking is not allowed on a JoinedStream.");
            }
        }

        public override void Flush()
        {
            throw new InvalidOperationException("Flushing is not allowed on a JoinedStream.");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int totalBytesRead = 0;

            while (count > 0 && _currentStreamIndex < _streams.Length)
            {
                int numBytesRead = _streams[_currentStreamIndex].Read(buffer, offset, count);

                count -= numBytesRead;
                offset += numBytesRead;
                totalBytesRead += numBytesRead;

                if (count > 0)
                {
                    _streams[_currentStreamIndex].Close();
                    _currentStreamIndex++;
                }
            }

            _currentPosition += totalBytesRead;

            return totalBytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException("Seeking is not allowed on a JoinedStream.");
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException("Setting length is not allowed on a JoinedStream.");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException("Writing is not allowed on a JoinedStream.");
        }
    }
}