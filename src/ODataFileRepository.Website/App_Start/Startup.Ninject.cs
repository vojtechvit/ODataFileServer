using Ninject;
using Ninject.Web.Common;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.FileSystem;
using System.Diagnostics.CodeAnalysis;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class Ninject
        {
            [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope",
                Justification = "Kernel must be disposed by caller.")]
            public static IKernel GetConfiguredKernel()
            {
                var kernel = new StandardKernel();

                kernel.Bind<IFileDataAccess>().To<FileSystemFileDataAccess>().InRequestScope();

                return kernel;
            }
        }
    }
}