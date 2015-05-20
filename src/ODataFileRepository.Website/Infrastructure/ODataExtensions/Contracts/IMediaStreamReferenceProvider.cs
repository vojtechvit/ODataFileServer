using Microsoft.OData.Core;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Website.Infrastructure.ODataExtensions.Contracts
{
    public interface IMediaStreamReferenceProvider
    {
        ODataStreamReferenceValue GetMediaStreamReference(
            EntityInstanceContext entity,
            ODataSerializerContext context);
    }
}