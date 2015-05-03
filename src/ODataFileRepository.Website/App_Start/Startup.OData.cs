using Microsoft.OData.Edm;
using ODataFileRepository.Website.Models;
using System.Web.OData.Builder;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class OData
        {
            public static IEdmModel CreateModel()
            {
                var modelBuilder = new ODataConventionModelBuilder();
                modelBuilder.EnableLowerCamelCase();

                DefineTypes(modelBuilder);
                DefineEntityContainer(modelBuilder);

                return modelBuilder.GetEdmModel();
            }

            private static void DefineTypes(ODataModelBuilder modelBuilder)
            {
                var fileType = modelBuilder.EntityType<File>();
                fileType.Name = "file";
                fileType.Namespace = "type";
            }

            private static void DefineEntityContainer(ODataModelBuilder modelBuilder)
            {
                modelBuilder.Namespace = "container";
                modelBuilder.ContainerName = "odataFileRepository";

                modelBuilder.EntitySet<File>("files");
            }
        }
    }
}