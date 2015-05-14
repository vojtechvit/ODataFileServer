using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using ODataFileRepository.Website.Models;
using System;
using System.Web.OData.Builder;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class OData
        {
            public const string TYPE_NAMESPACE = "type";

            public static IEdmModel CreateModel()
            {
                var model = new EdmModel();

                var fileType = new EdmEntityType(TYPE_NAMESPACE, "file", null, false, false, true);
                model.AddElement(fileType);
                model.SetDescriptionAnnotation(fileType, "Represents a file in the file repository.");

                var entityContainer = new EdmEntityContainer("container", "fileRepository");
                var filesEntitySet = new EdmEntitySet(entityContainer, "files", fileType);
                model.AddElement(entityContainer);

                return model;
            }
        }
    }
}