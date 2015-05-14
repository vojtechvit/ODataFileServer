using System.Net.Http;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public static class HttpRequestMessageExtensions
    {
        public static IMediaLinkProvider GetMediaEntityStreamProvider(this HttpRequestMessage request)
        {
            var key = typeof(IMediaLinkProvider).FullName;
            object value;

            if (request.Properties.TryGetValue(key, out value))
            {
                return (IMediaLinkProvider)value;
            }

            return null;
        }

        public static void SetMediaEntityStreamProvider(this HttpRequestMessage request, IMediaLinkProvider provider)
        {
            var key = typeof(IMediaLinkProvider).FullName;
            request.Properties[key] = provider;
        }
    }
}