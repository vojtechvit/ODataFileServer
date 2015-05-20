using Newtonsoft.Json;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.DomainModels;
using ODataFileRepository.Website.DomainModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess
{
    public sealed class FileSystemFileDataAccess : IFileDataAccess
    {
        private static DirectoryInfo _appDataDirectory = GetAppDataDirectory();

        public async Task<IFileMetadata> CreateAsync(string fullName, string mediaType, Stream stream)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));
            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName + ".json"));

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                var metadata = new FileMetadata
                {
                    FullName = fullName,
                    MediaType = mediaType
                };

                var metadataJson = JsonConvert.SerializeObject(metadata);
                var metadataJsonBytes = Encoding.UTF8.GetBytes(metadataJson);

                using (var fileStream = new FileStream(metadataFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await fileStream.WriteAsync(metadataJsonBytes, 0, metadataJsonBytes.Length);
                }

                return metadata;
            }
            catch (IOException exception)
            {
                if (!exception.Message.Contains("exists"))
                {
                    throw;
                }

                binaryFile.Delete();
                metadataFile.Delete();

                throw new ResourceAlreadyExistsException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File '{0}' already exists.",
                        binaryFile.Name),
                    exception);
            }
            catch (Exception exception)
            {
                binaryFile.Delete();
                metadataFile.Delete();

                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not create file '{0}' due to an internal error in the data access layer.",
                        binaryFile.Name),
                    exception);
            }
        }

        public Task<bool> ExistsAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var binaryFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));

            return Task.FromResult(binaryFile.Exists);
        }

        public async Task<IReadOnlyList<IFileMetadata>> GetAllAsync()
        {
            try
            {
                var metadataFiles = _appDataDirectory.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly);

                var readTasks = new List<Task<IFileMetadata>>();

                foreach (var metadataFile in metadataFiles)
                {
                    readTasks.Add(Task.Run<IFileMetadata>(async () =>
                    {
                        using (var fileStream = metadataFile.OpenRead())
                        using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
                        {
                            string metadataString = await reader.ReadToEndAsync();
                            return JsonConvert.DeserializeObject<FileMetadata>(metadataString);
                        }
                    }));
                }

                return await Task.WhenAll(readTasks);
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    "Could not get files due to an internal error in the data access layer.",
                    exception);
            }
        }

        public async Task<IFileMetadata> GetMetadataAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName + ".json"));

            try
            {
                using (var fileStream = metadataFile.OpenRead())
                using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
                {
                    string metadataString = await reader.ReadToEndAsync();
                    return JsonConvert.DeserializeObject<FileMetadata>(metadataString);
                }
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not get metadata of file '{0}' due to an internal error in the data access layer.",
                        metadataFile.Name),
                    exception);
            }
        }

        public async Task<StreamWithMetadata> GetStreamAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName + ".json"));
            var binaryFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));

            try
            {
                IFileMetadata metadata = null;

                using (var fileStream = metadataFile.OpenRead())
                using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
                {
                    string metadataString = await reader.ReadToEndAsync();
                    metadata = JsonConvert.DeserializeObject<FileMetadata>(metadataString);
                }

                return new StreamWithMetadata(() => binaryFile.OpenRead(), metadata.MediaType);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not get stream of file '{0}' due to an internal error in the data access layer.",
                        binaryFile.Name),
                    exception);
            }
        }

        public async Task UpdateMetadataAsync(IFileMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName + ".json", metadata.FullName));

            if (!metadataFile.Exists)
            {
                throw new ResourceNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File with full name: '{0}' does not exist.",
                        metadataFile.Name));
            }

            try
            {
                var metadataJson = JsonConvert.SerializeObject(metadata);
                var metadataJsonBytes = Encoding.UTF8.GetBytes(metadataJson);

                using (var fileStream = new FileStream(metadataFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.WriteAsync(metadataJsonBytes, 0, metadataJsonBytes.Length);
                }
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not update metadata of file '{0}' due to an internal error in the data access layer.",
                        metadata.FullName),
                    exception);
            }
        }

        public async Task UpdateStreamAsync(string fullName, string mediaType, Stream stream)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));
            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName + ".json"));

            if (!binaryFile.Exists)
            {
                throw new ResourceNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File with full name: '{0}' does not exist.",
                        fullName));
            }

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                FileMetadata metadata = null;

                using (var fileStream = metadataFile.OpenRead())
                using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
                {
                    string metadataString = await reader.ReadToEndAsync();
                    metadata = JsonConvert.DeserializeObject<FileMetadata>(metadataString);
                }

                metadata.MediaType = mediaType;

                var metadataJson = JsonConvert.SerializeObject(metadata);
                var metadataJsonBytes = Encoding.UTF8.GetBytes(metadataJson);

                using (var fileStream = new FileStream(metadataFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await fileStream.WriteAsync(metadataJsonBytes, 0, metadataJsonBytes.Length);
                }
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not update stream of file '{0}' due to an internal error in the data access layer.",
                        binaryFile.Name),
                    exception);
            }
        }

        public Task DeleteAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var binaryFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName));
            var metadataFile = new FileInfo(Path.Combine(_appDataDirectory.FullName, fullName + ".json"));

            if (!binaryFile.Exists)
            {
                throw new ResourceNotFoundException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "File with full name: '{0}' does not exist.",
                        binaryFile.Name));
            }

            try
            {
                binaryFile.Delete();
                metadataFile.Delete();

                return Task.FromResult(true);
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not delete file '{0}' due to an internal error in the data access layer.",
                        binaryFile.Name),
                    exception);
            }
        }

        private static DirectoryInfo GetAppDataDirectory()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }
    }
}