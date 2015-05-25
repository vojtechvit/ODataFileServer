using Newtonsoft.Json;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.DataAccessModels;
using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public sealed class FileSystemFileDataAccess : IFileDataAccess
    {
        public async Task<IFileMetadata> CreateAsync(
            string identifier,
            string name,
            string mediaType,
            Stream stream)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = GetBinaryFileInfo(identifier);
            var metadataFile = GetMetadataFileInfo(identifier);

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                var metadata = new FileMetadata
                {
                    Id = identifier,
                    Name = name
                };

                await SaveMetadata(metadataFile, metadata);

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

            var binaryFile = new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, fullName));

            return Task.FromResult(binaryFile.Exists);
        }

        public async Task<IReadOnlyList<IFileMetadata>> GetAllAsync()
        {
            try
            {
                var metadataFiles = FileSystemHelpers.AppDataDirectory.EnumerateFiles("*.json", SearchOption.TopDirectoryOnly);

                var readTasks = new List<Task<IFileMetadata>>();

                foreach (var metadataFile in metadataFiles)
                {
                    readTasks.Add(Task.Run<IFileMetadata>(async () =>
                    {
                        return await ReadMetadata(metadataFile);
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

        public async Task<IFileMetadata> GetAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var metadataFile = new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, fullName + ".json"));

            try
            {
                return await ReadMetadata(metadataFile);
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

        public async Task<LazyServiceStream> GetStreamAsync(string fullName)
        {
            if (fullName == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var metadataFile = new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, fullName + ".json"));
            var binaryFile = new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, fullName));

            try
            {
                var metadata = await ReadMetadata(metadataFile);

                return new LazyServiceStream(() => binaryFile.OpenRead(), metadata.MediaType);
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

        public async Task UpdateAsync(IFileMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            var metadataFile = GetMetadataFileInfo(metadata.Id);

            if (!metadataFile.Exists)
            {
                Exceptions.ResourceNotFound(metadataFile.Name);
            }

            try
            {
                await SaveMetadata(metadataFile, metadata);
            }
            catch (Exception exception)
            {
                throw new DataAccessException(
                    string.Format(
                        CultureInfo.InvariantCulture,
                        "Could not update metadata of file '{0}' due to an internal error in the data access layer.",
                        metadata.Id),
                    exception);
            }
        }

        public async Task UpdateStreamAsync(string identifier, string mediaType, Stream stream)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = GetBinaryFileInfo(identifier);
            var metadataFile = GetMetadataFileInfo(identifier);

            if (!binaryFile.Exists)
            {
                Exceptions.ResourceNotFound(binaryFile.Name);
            }

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                var metadata = await ReadMetadata(metadataFile);

                metadata.MediaType = mediaType;

                await SaveMetadata(metadataFile, metadata);
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
        public Task DeleteAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            var binaryFile = GetBinaryFileInfo(identifier);
            var metadataFile = GetMetadataFileInfo(identifier);

            if (!binaryFile.Exists)
            {
                Exceptions.ResourceNotFound(binaryFile.Name);
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

        private static FileInfo GetBinaryFileInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "files", identifier, "content"));
        }

        private static FileInfo GetMetadataFileInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "files", identifier, "metadata.json"));
        }

        private static async Task<FileMetadata> ReadMetadata(FileInfo metadataFile)
        {
            FileMetadata metadata = null;

            using (var fileStream = metadataFile.OpenRead())
            using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
            {
                string metadataString = await reader.ReadToEndAsync();
                metadata = JsonConvert.DeserializeObject<FileMetadata>(metadataString);
            }

            return metadata;
        }

        private static async Task SaveMetadata(FileInfo metadataFile, IFileMetadata metadata)
        {
            var metadataJson = JsonConvert.SerializeObject(metadata);
            var metadataJsonBytes = Encoding.UTF8.GetBytes(metadataJson);

            using (var fileStream = new FileStream(metadataFile.FullName, FileMode.CreateNew, FileAccess.Write))
            {
                await fileStream.WriteAsync(metadataJsonBytes, 0, metadataJsonBytes.Length);
            }
        }
    }
}