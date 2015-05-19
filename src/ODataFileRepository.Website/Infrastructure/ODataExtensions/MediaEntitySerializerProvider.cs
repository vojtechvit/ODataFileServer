using Microsoft.OData.Edm;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public class MediaEntitySerializerProvider : DefaultODataSerializerProvider
    {
        public override ODataEdmTypeSerializer GetEdmTypeSerializer(IEdmTypeReference edmType)
        {
            if (edmType.IsEntity())
            {
                return new MediaEntityTypeSerializer(this);
            }

            return base.GetEdmTypeSerializer(edmType);
        }
    }
}