using Ninject;
using Ninject.Web.Common;
using ODataFileRepository.Website.DataAccess;
using ODataFileRepository.Website.DataAccess.Contracts;

namespace ODataFileRepository.Website
{
    public partial class Startup
    {
        private static class Ninject
        {
            public static IKernel GetConfiguredKernel()
            {
                var kernel = new StandardKernel();

                kernel.Bind<IFileDataAccess>().To<FileSystemFileDataAccess>().InRequestScope();

                return kernel;
            }
        }
    }
}