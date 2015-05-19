using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using Microsoft.OData.Edm.Library.Values;
using ODataFileRepository.Website.Infrastructure.ODataExtensions;
using ODataFileRepository.Website.ServiceModels;
using System.Web.OData;
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
                model.SetDescriptionAnnotation(fileType, "Represents a file in the file repository.");
                model.SetAnnotationValue(fileType, new ClrTypeAnnotation(typeof(File)));

                model.AddElement(fileType);

                var fullNameProperty = fileType.AddStructuralProperty("fullName", EdmPrimitiveTypeKind.String, false);
                model.SetDescriptionAnnotation(fullNameProperty, "The unique full name of the file.");
                model.SetAnnotationValue(fullNameProperty, new ClrPropertyInfoAnnotation(typeof(File).GetProperty("FullName")));
                fileType.AddKeys(fullNameProperty);

                var entityContainer = new EdmEntityContainer("container", "fileRepository");
                entityContainer.AddEntitySet("files", fileType);

                model.AddElement(entityContainer);

                return model;
            }
        }
    }
}