using Microsoft.OData.Core;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public class MediaEntityTypeSerializer : ODataEntityTypeSerializer
    {
        public MediaEntityTypeSerializer(ODataSerializerProvider serializerProvider)
            : base(serializerProvider)
        {
        }

        public override ODataEntry CreateEntry(
            SelectExpandNode selectExpandNode,
            EntityInstanceContext entityInstanceContext)
        {
            var entry = base.CreateEntry(selectExpandNode, entityInstanceContext);
            var context = entityInstanceContext.SerializerContext;

            // if the model doesn't have a stream, then this isn't a media link entry
            if (!entityInstanceContext.EntityType.HasStream)
            {
                return entry;
            }

            var entity = entityInstanceContext.EntityInstance;

            // must have an entity
            if (entity == null)
            {
                return entry;
            }

            // get the metadata provider associated with the current request
            var provider = entityInstanceContext.Request.GetMediaStreamReferenceProvider();

            // need metadata to construct the media link enty
            if (provider == null)
            {
                return entry;
            }

            // attach the media link entry
            var mediaResource = provider.GetMediaStreamReference(entityInstanceContext, context);
            entry.MediaResource = mediaResource;

            return entry;
        }
    }
}