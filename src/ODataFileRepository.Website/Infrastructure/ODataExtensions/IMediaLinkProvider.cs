using Microsoft.OData.Core;
using System.Web.OData;
using System.Web.OData.Formatter.Serialization;

namespace ODataFileRepository.Infrastructure.ODataExtensions
{
    public interface IMediaLinkProvider
    {
        ODataStreamReferenceValue GetMediaLinks(
            EntityInstanceContext entity,
            ODataSerializerContext context);
    }
}