using ODataFileRepository.Website.Infrastructure.ODataExtensions.Contracts;
using System.Net.Http;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public static class HttpRequestMessageExtensions
    {
        public static IMediaStreamReferenceProvider GetMediaStreamReferenceProvider(
            this HttpRequestMessage request)
        {
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
            var key = typeof(IMediaStreamReferenceProvider).FullName;
            request.Properties[key] = provider;
        }
    }
}