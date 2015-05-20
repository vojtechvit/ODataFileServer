using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using ODataFileRepository.Website.ServiceModels;
using System;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Web.OData.Builder;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class OData
        {
            private const string TypeNamespace = "type";

            public static IEdmModel CreateModel()
            {
                var modelBuilder = new ODataConventionModelBuilder();
                modelBuilder.EnableLowerCamelCase();
                modelBuilder.Namespace = "container";
                modelBuilder.ContainerName = "fileRepository";

                var fileType = modelBuilder.EntityType<File>();
                fileType.Name = Camelize(fileType.Name);
                fileType.Namespace = TypeNamespace;

                modelBuilder.EntitySet<File>("files");

                var model = modelBuilder.GetEdmModel() as EdmModel;

                var edmFileType = model.FindDeclaredType(QualifiedTypeName(fileType.Name)) as EdmEntityType;
                SetPrivateFieldValue(edmFileType, "hasStream", true);

                model.SetDescriptionAnnotation(edmFileType, "Represents a file in the file repository.");

                return model;
            }

            private static string QualifiedTypeName(string localTypeName)
            {
                return string.Join(".", TypeNamespace, localTypeName);
            }

            private static string Camelize(string text)
            {
                if (text == null)
                {
                    throw new ArgumentNullException("text");
                }

                if (text.Length == 0)
                {
                    return text;
                }

                var stringBuilder = new StringBuilder(text);
                stringBuilder[0] = char.ToLowerInvariant(text[0]);

                return stringBuilder.ToString();
            }

            private static void SetPrivateFieldValue<T>(object obj, string propName, T val)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }

                Type t = obj.GetType();
                FieldInfo fi = null;

                while (fi == null && t != null)
                {
                    fi = t.GetField(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    t = t.BaseType;
                }

                if (fi == null)
                {
                    throw new ArgumentOutOfRangeException(
                        "propName",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Field {0} was not found in Type {1}",
                            propName,
                            obj.GetType().FullName));
                }

                fi.SetValue(obj, val);
            }
        }
    }
}