using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.DomainModels;
using ODataFileRepository.Website.DomainModels.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess
{
    public sealed class FileSystemFileDataAccess : IFileDataAccess
    {
        private static DirectoryInfo _appDataDirectory = GetAppDataDirectory();

        public Task<IFileMetadata> CreateAsync(string fullName, string mediaType, Stream stream)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(string fullName)
        {
            var file = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));

            if (!file.Exists)
            {
                throw new ResourceNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File with full name: '{0}' does not exist.",
                        file.Name));
            }

            try
            {
                file.Delete();
                return Task.FromResult(true);
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not delete file '{0}' due to an internal error in the data access layer.",
                        file.Name),
                    exception);
            }
        }

        public Task<bool> ExistsAsync(string fullName)
        {
            try
            {
                return Task.FromResult(
                _appDataDirectory.EnumerateFiles()
                .Any(f => f.Name.Equals(fullName, StringComparison.OrdinalIgnoreCase)));
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not verify existence of file '{0}' due to an internal error in the data access layer.",
                        fullName),
                    exception);
            }
        }

        public Task<IReadOnlyList<IFileMetadata>> GetAllAsync()
        {
            try
            {
                return Task.FromResult((IReadOnlyList<IFileMetadata>)
                _appDataDirectory.EnumerateFiles()
                .Select(f => (IFileMetadata)new FileMetadata
                {
                    FullName = f.Name,
                    MediaType = f.Extension
                })
                .ToList());
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    "Could not get files due to an internal error in the data access layer.",
                    exception);
            }
        }

        public Task<IFileMetadata> GetMetadataAsync(string fullName)
        {
            var file = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));

            if (!file.Exists)
            {
                return Task.FromResult((IFileMetadata)null);
            }

            try
            {
                return Task.FromResult((IFileMetadata)new FileMetadata
                {
                    FullName = file.Name,
                    MediaType = file.Extension
                });
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not get file '{0}' due to an internal error in the data access layer.",
                        file.Name),
                    exception);
            }
        }

        public Task<Stream> GetStreamAsync(string fullName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateMetadataAsync(IFileMetadata file)
        {
            throw new NotImplementedException();
        }

        private static DirectoryInfo GetAppDataDirectory()
        {
            return new DirectoryInfo(@"C:\Users\iam_000\Source\Repos\ODataFileServer\src\ODataFileRepository.Website\App_Data");
            //return new DirectoryInfo(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }
    }
}