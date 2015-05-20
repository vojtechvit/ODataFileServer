using ODataFileRepository.Website.Infrastructure.ODataExtensions.Contracts;
using System;
using System.Net.Http;

namespace ODataFileRepository.Website.Infrastructure.ODataExtensions
{
    public static class HttpRequestMessageExtensions
    {
        public static IMediaStreamReferenceProvider GetMediaStreamReferenceProvider(
            this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var key = typeof(IMediaStreamReferenceProvider).FullName;
            object value;

            if (request.Properties.TryGetValue(key, out value))
            {
                return (IMediaStreamReferenceProvider)value;
            }

            return null;
        }

        public static void SetMediaStreamReferenceProvider(
            this HttpRequestMessage request,
            IMediaStreamReferenceProvider provider)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            var key = typeof(IMediaStreamReferenceProvider).FullName;
            request.Properties[key] = provider;
        }
    }
}