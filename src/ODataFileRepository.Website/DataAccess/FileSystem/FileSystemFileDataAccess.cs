using Newtonsoft.Json;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccessModels;
using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public sealed class FileSystemFileDataAccess : IFileDataAccess
    {
        private static readonly DirectoryInfo _filesDirectory = new DirectoryInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "Files"));

        public FileSystemFileDataAccess()
        {
            if (!FilesDirectory.Exists)
            {
                FilesDirectory.Create();
            }
        }

        internal static DirectoryInfo FilesDirectory
        {
            get { return _filesDirectory; }
        }

        public async Task<IFileMetadata> CreateAsync(
            string identifier,
            string name,
            string mediaType,
            long size,
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

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than zero.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var fileDirectory = GetFileDirectoryInfo(identifier);
            var binaryFile = GetFileBinaryFileInfo(identifier);
            var metadataFile = GetFileMetadataFileInfo(identifier);

            if (fileDirectory.Exists)
            {
                throw ExceptionHelper.ResourceAlreadyExists(identifier);
            }

            try
            {
                fileDirectory.Create();

                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                var metadata = new FileMetadata
                {
                    Id = identifier,
                    Name = name,
                    MediaType = mediaType,
                    Size = size
                };

                await SaveFileMetadataAsync(metadata);

                return metadata;
            }
            catch (Exception exception)
            {
                fileDirectory.Delete();
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public Task<bool> ExistsAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return Task.FromResult(GetFileDirectoryInfo(identifier).Exists);
        }

        public async Task<IReadOnlyList<IFileMetadata>> GetAllAsync()
        {
            try
            {
                var fileDirectories = FilesDirectory.EnumerateDirectories();

                var readTasks = new List<Task<FileMetadata>>();

                foreach (var fileDirectory in fileDirectories)
                {
                    var metadataFile = GetFileMetadataFileInfo(fileDirectory.Name);

                    readTasks.Add(ReadFileMetadataAsync(metadataFile));
                }

                return await Task.WhenAll(readTasks);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public async Task<IFileMetadata> GetAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            var metadataFile = GetFileMetadataFileInfo(identifier);

            try
            {
                return await ReadFileMetadataAsync(metadataFile);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public async Task<MetadataStream<IFileMetadata>> GetStreamAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("fullName");
            }

            var metadataFile = GetFileMetadataFileInfo(identifier);
            var binaryFile = GetFileBinaryFileInfo(identifier);

            try
            {
                var metadata = await ReadFileMetadataAsync(metadataFile);

                return new MetadataStream<IFileMetadata>(binaryFile.OpenRead(), metadata);
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public async Task UpdateAsync(IFileMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            var metadataFile = GetFileMetadataFileInfo(metadata.Id);

            if (!metadataFile.Exists)
            {
                throw ExceptionHelper.ResourceNotFound(metadataFile.Name);
            }

            try
            {
                await SaveFileMetadataAsync(metadata);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public async Task UpdateStreamAsync(
            string identifier, 
            string mediaType, 
            long size,
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

            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException("size", "size must be greater than zero.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var binaryFile = GetFileBinaryFileInfo(identifier);
            var metadataFile = GetFileMetadataFileInfo(identifier);

            if (!binaryFile.Exists)
            {
                throw ExceptionHelper.ResourceNotFound(binaryFile.Name);
            }

            try
            {
                using (var fileStream = new FileStream(binaryFile.FullName, FileMode.Create, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                var metadata = await ReadFileMetadataAsync(metadataFile);

                metadata.MediaType = mediaType;
                metadata.Size = size;

                await SaveFileMetadataAsync(metadata);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public Task DeleteAsync(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            var fileDirectory = GetFileDirectoryInfo(identifier);

            if (!fileDirectory.Exists)
            {
                throw ExceptionHelper.ResourceNotFound(identifier);
            }

            try
            {
                fileDirectory.Delete(true);

                return Task.FromResult(true);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        internal static DirectoryInfo GetFileDirectoryInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new DirectoryInfo(Path.Combine(FilesDirectory.FullName, identifier));
        }

        internal static FileInfo GetFileBinaryFileInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new FileInfo(Path.Combine(GetFileDirectoryInfo(identifier).FullName, "content"));
        }

        internal static FileInfo GetFileMetadataFileInfo(string identifier)
        {
            if (identifier == null)
            {
                throw new ArgumentNullException("identifier");
            }

            return new FileInfo(Path.Combine(GetFileDirectoryInfo(identifier).FullName, "metadata.json"));
        }

        internal static async Task<FileMetadata> ReadFileMetadataAsync(FileInfo metadataFile)
        {
            FileMetadata metadata = null;

            using (var reader = new StreamReader(metadataFile.FullName, Encoding.UTF8))
            {
                string metadataString = await reader.ReadToEndAsync();
                metadata = JsonConvert.DeserializeObject<FileMetadata>(metadataString);
            }

            return metadata;
        }

        internal static async Task SaveFileMetadataAsync(IFileMetadata metadata)
        {
            var metadataFile = GetFileMetadataFileInfo(metadata.Id);
            var metadataJson = JsonConvert.SerializeObject(metadata);
            var metadataJsonBytes = Encoding.UTF8.GetBytes(metadataJson);

            using (var fileStream = new FileStream(metadataFile.FullName, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(metadataJsonBytes, 0, metadataJsonBytes.Length);
            }
        }
    }
}