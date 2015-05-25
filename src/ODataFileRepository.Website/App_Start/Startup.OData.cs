using Microsoft.OData.Edm;
using Microsoft.OData.Edm.Library;
using ODataFileRepository.Website.ServiceModels;
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.OData.Builder;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class OData
        {
            private const string ContainerNamespace = "container";

            private const string TypeNamespace = "type";

            public static IEdmModel CreateModel()
            {
                var modelBuilder = new ODataConventionModelBuilder();
                modelBuilder.EnableLowerCamelCase();
                modelBuilder.Namespace = ContainerNamespace;
                modelBuilder.ContainerName = "fileRepository";

                var fileType = modelBuilder.EntityType<File>();
                fileType.Name = Camelize(fileType.Name);
                fileType.Namespace = TypeNamespace;

                var fileVersionType = modelBuilder.EntityType<FileVersion>();
                fileVersionType.Name = Camelize(fileVersionType.Name);
                fileVersionType.Namespace = TypeNamespace;

                var uploadSessionType = modelBuilder.EntityType<UploadSession>();
                uploadSessionType.Name = Camelize(uploadSessionType.Name);
                uploadSessionType.Namespace = TypeNamespace;

                var startSessionAction = fileType.Collection.Action("startSession");
                startSessionAction.Parameter<string>("fileName");
                startSessionAction.ReturnsFromEntitySet<UploadSession>("uploadSessions");

                modelBuilder.EntitySet<File>("files");
                modelBuilder.EntitySet<UploadSession>("uploadSessions");

                var model = modelBuilder.GetEdmModel() as EdmModel;

                var edmFileType = model.FindDeclaredType(QualifiedTypeName(fileType.Name)) as EdmEntityType;
                SetPrivateFieldValue(edmFileType, "hasStream", true);

                var edmStartSessionAction = model.FindDeclaredOperations(QualifiedContainerName(startSessionAction.Name)).First() as EdmAction;
                SetPrivatePropertyValue(edmStartSessionAction, "Namespace", "upload");

                model.SetDescriptionAnnotation(edmFileType, "Represents a file in the file repository.");

                return model;
            }

            private static string QualifiedContainerName(string localTypeName)
            {
                return string.Join(".", ContainerNamespace, localTypeName);
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

            private static void SetPrivateFieldValue<T>(object obj, string fieldName, T value)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }

                Type type = obj.GetType();
                FieldInfo fieldInfo = null;

                while (fieldInfo == null && type != null)
                {
                    fieldInfo = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    type = type.BaseType;
                }

                if (fieldInfo == null)
                {
                    throw new ArgumentOutOfRangeException(
                        "fieldName",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Field {0} was not found in Type {1}",
                            fieldName,
                            obj.GetType().FullName));
                }

                fieldInfo.SetValue(obj, value);
            }

            private static void SetPrivatePropertyValue<T>(object obj, string propertyName, T value)
            {
                if (obj == null)
                {
                    throw new ArgumentNullException("obj");
                }

                Type type = obj.GetType();
                MethodInfo methodInfo = null;

                while (methodInfo == null && type != null)
                {
                    methodInfo = type.GetMethod("set_" + propertyName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    type = type.BaseType;
                }

                if (methodInfo == null)
                {
                    throw new ArgumentOutOfRangeException(
                        "methodInfo",
                        string.Format(
                            CultureInfo.InvariantCulture,
                            "Method {0} was not found in Type {1}",
                            propertyName,
                            obj.GetType().FullName));
                }

                methodInfo.Invoke(obj, new object[] { value });
            }
        }
    }
}