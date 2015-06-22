using ODataFileRepository.Website.DataAccess.Exceptions;
using System;
using System.Globalization;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public static class ExceptionHelper
    {
        public static ResourceNotFoundException ResourceNotFound(string identifier)
        {
            return new ResourceNotFoundException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Resource with identifier '{0}' does not exist.",
                    identifier));
        }

        public static DataAccessException OtherError(Exception exception)
        {
            return new DataAccessException(
                "Could not perform operation to an internal error in the data access layer.",
                exception);
        }

        public static ResourceAlreadyExistsException ResourceAlreadyExists(string resourceIdentifier)
        {
            return new ResourceAlreadyExistsException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Resource with identifier '{0}' already exists.",
                    resourceIdentifier));
        }

        public static ResourceAlreadyExistsException ResourceAlreadyExists(string resourceIdentifier, Exception exception)
        {
            return new ResourceAlreadyExistsException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Resource with identifier '{0}' already exists.",
                    resourceIdentifier),
                exception);
        }
    }
}