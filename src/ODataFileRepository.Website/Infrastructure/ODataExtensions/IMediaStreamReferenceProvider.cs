using Microsoft.OData.Core;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public interface IMediaStreamReferenceProvider
    {
        ODataStreamReferenceValue GetMediaStreamReference(
            EntityInstanceContext entity,
            ODataSerializerContext context);
    }
}