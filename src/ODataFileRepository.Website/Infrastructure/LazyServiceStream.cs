using System;
using System.IO;

namespace ODataFileRepository.Website.Infrastructure
{
    public class LazyMediaStream : LazyStream
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMediaStream"/> class.
        /// </summary>
        /// <param name="streamFactory">The <see cref="Func{T}">factory method</see> used to retrieve the underlying <see cref="Stream">stream</see>.</param>
        /// <param name="mediaType">The MIME type of the stream content.</param>
        public LazyMediaStream(Func<Stream> streamFactory, string mediaType, string fileName)
            : this(new Lazy<Stream>(streamFactory), mediaType, fileName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LazyMediaStream"/> class.
        /// </summary>
        /// <param name="lazyStream">The underlying <see cref="Lazy{T}">on-demand</see> <see cref="Stream">stream</see>.</param>
        /// <param name="mediaType">The MIME type of the stream content.</param>
        public LazyMediaStream(Lazy<Stream> lazyStream, string mediaType, string fileName)
            : base(lazyStream)
        {
            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            MediaType = mediaType;
            FileName = fileName;
        }

        public string MediaType { get; private set; }

        public string FileName { get; private set; }
    }
}