using Microsoft.OData.Core;
using ODataFileRepository.Website.Infrastructure.ODataExtensions.Contracts;
using System;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Website.Infrastructure.ODataExtensions
{
    public class DefaultMediaStreamReferenceProvider : IMediaStreamReferenceProvider
    {
        public ODataStreamReferenceValue GetMediaStreamReference(
            EntityInstanceContext entity,
            ODataSerializerContext context)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            string mediaType = null;
            string eTag = null;

            var mediaTypeHolder = entity.EntityInstance as IMediaTypeHolder;

            if (mediaTypeHolder != null)
            {
                mediaType = mediaTypeHolder.MediaType;
            }

            return new ODataStreamReferenceValue
            {
                ContentType = mediaType,
                ETag = eTag
            };
        }
    }
}