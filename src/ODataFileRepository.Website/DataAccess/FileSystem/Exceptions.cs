using ODataFileRepository.Website.DataAccess.Exceptions;
using System.Globalization;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public static class Exceptions
    {
        public static void ResourceNotFound(string identifier)
        {
            throw new ResourceNotFoundException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "File with identifier: '{0}' does not exist.",
                    identifier));
        }
    }
}