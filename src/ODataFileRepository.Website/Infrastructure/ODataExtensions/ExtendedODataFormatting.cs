using ODataFileRepository.Infrastructure.ODataExtensions;
using System;
using System.Collections.Generic;
using System.Web.OData;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;

namespace ODataFileRepository.Website.Infrastructure.ODataExtensions
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ExtendedODataFormattingAttribute : ODataFormattingAttribute
    {
        public override IList<ODataMediaTypeFormatter> CreateODataFormatters()
        {
            return ODataMediaTypeFormatters.Create(new MediaEntitySerializerProvider(), new DefaultODataDeserializerProvider());
        }
    }
}