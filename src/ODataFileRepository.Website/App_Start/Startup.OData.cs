using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;

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

                var fullNameProperty = fileType.AddStructuralProperty("fullName", EdmPrimitiveTypeKind.String, false);
                model.SetDescriptionAnnotation(fullNameProperty, "The unique full name of the file.");
                fileType.AddKeys(fullNameProperty);

                var mediaTypeProperty = fileType.AddStructuralProperty("mediaType", EdmPrimitiveTypeKind.String, false);
                model.SetDescriptionAnnotation(mediaTypeProperty, "The media type of the file.");

                var entityContainer = new EdmEntityContainer("container", "fileRepository");
                entityContainer.AddEntitySet("files", fileType);

                model.AddElement(entityContainer);

                return model;
            }
        }
    }
}