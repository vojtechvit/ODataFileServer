using Newtonsoft.Json;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.DataAccessModels;
using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.FileSystem
{
    public sealed class FileSystemUploadSessionDataAccess : IUploadSessionDataAccess
    {
        private static readonly DirectoryInfo _uploadSessionsDirectory = new DirectoryInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "UploadSessions"));

        private static readonly TimeSpan _uploadSessionTimeout = TimeSpan.FromDays(7);

        public FileSystemUploadSessionDataAccess()
        {
            if (!UploadSessionsDirectory.Exists)
            {
                UploadSessionsDirectory.Create();
            }
        }

        internal static DirectoryInfo UploadSessionsDirectory
        {
            get { return _uploadSessionsDirectory; }
        }

        public async Task<IUploadSession> CreateAsync(
            string uploadSessionIdentifier,
            string fileIdentifier,
            string fileName)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            if (string.IsNullOrWhiteSpace(uploadSessionIdentifier))
            {
                throw new ArgumentException(
                    "fileIdentifier must not be empty nor whitespace.", 
                    "uploadSessionIdentifier");
            }

            if (fileIdentifier == null)
            {
                throw new ArgumentNullException("fileIdentifier");
            }

            if (string.IsNullOrWhiteSpace(fileIdentifier))
            {
                throw new ArgumentOutOfRangeException("fileIdentifier", "fileIdentifier must not be empty nor whitespace.");
            }

            var uploadSession = new UploadSession
            {
                Id = uploadSessionIdentifier,
                FileIdentifier = fileIdentifier,
                FileName = fileName,
                ExpirationDateTime = DateTimeOffset.UtcNow.Add(_uploadSessionTimeout)
            };

            var uploadSessionDirectory = GetUploadSessionDirectoryInfo(uploadSessionIdentifier);
            var uploadSessionMetadataFile = GetUploadSessionMetadataFileInfo(uploadSessionIdentifier);

            if (uploadSessionDirectory.Exists)
            {
                throw ExceptionHelper.ResourceAlreadyExists(uploadSessionIdentifier);
            }

            try
            {
                uploadSessionDirectory.Create();
                await SaveUploadSessionMetadataAsync(uploadSession);

                return uploadSession;
            }
            catch (Exception exception)
            {
                uploadSessionDirectory.Delete();
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public Task DeleteAsync(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            var uploadSessionDirectory = GetUploadSessionDirectoryInfo(uploadSessionIdentifier);

            if (!uploadSessionDirectory.Exists)
            {
                throw ExceptionHelper.ResourceNotFound(uploadSessionIdentifier);
            }

            try
            {
                uploadSessionDirectory.Delete(true);

                return Task.FromResult(true);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public Task<bool> ExistsAsync(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return Task.FromResult(GetUploadSessionDirectoryInfo(uploadSessionIdentifier).Exists);
        }

        public async Task<IReadOnlyList<IUploadSession>> GetAllAsync()
        {
            try
            {
                var uploadSessionDirectories = UploadSessionsDirectory.EnumerateDirectories();

                var readTasks = new List<Task<UploadSession>>();

                foreach (var uploadSessionDirectory in uploadSessionDirectories)
                {
                    var metadataFile = GetUploadSessionMetadataFileInfo(uploadSessionDirectory.Name);

                    readTasks.Add(ReadUploadSessionAsync(metadataFile));
                }

                return await Task.WhenAll(readTasks);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        public async Task<IUploadSession> GetAsync(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            var uploadSessionFile = GetUploadSessionMetadataFileInfo(uploadSessionIdentifier);

            try
            {
                return await ReadUploadSessionAsync(uploadSessionFile);
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

        public async Task<IUploadSession> UploadSegmentAsync(
            string uploadSessionIdentifier,
            string mediaType,
            long rangeFrom,
            long rangeTo,
            long rangeLength,
            Stream stream)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            if (mediaType == null)
            {
                throw new ArgumentNullException("mediaType");
            }

            if (rangeFrom < 0 || rangeFrom > rangeTo || rangeFrom > rangeLength - 1)
            {
                throw new ArgumentOutOfRangeException("rangeFrom", "rangeFrom must be greater than or equal to zero, less than or equal to rangeTo and less than rangeLength minus one.");
            }

            if (rangeTo < 0 || rangeTo < rangeFrom || rangeTo > rangeLength - 1)
            {
                throw new ArgumentOutOfRangeException("rangeTo", "rangeTo must be greater than or equal to zero, greater than or equal to rangeFrom and less than rangeLength minus one.");
            }

            if (rangeLength <= 0)
            {
                throw new ArgumentOutOfRangeException("rangeLength", "rangeLength must be greater than zero.");
            }

            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var segmentFile = GetSegmentFileInfo(uploadSessionIdentifier, rangeFrom, rangeTo);
            var uploadSession = await GetInternalAsync(uploadSessionIdentifier);

            if (uploadSession.FileMediaType == null)
            {
                var uploadSessionMetadataFile = GetUploadSessionMetadataFileInfo(uploadSessionIdentifier);
                uploadSession.FileMediaType = mediaType;
                uploadSession.FileSize = rangeLength;
                await SaveUploadSessionMetadataAsync(uploadSession);
            }
            else if (!mediaType.Equals(uploadSession.FileMediaType, StringComparison.Ordinal))
            {
                throw new InvalidMediaTypeException("mediaType is not equal to the initial media type of the uploaded file.");
            }
            else if (uploadSession.FileSize != rangeLength)
            {
                throw new InvalidRangeException("Content range length is not equal to the initial range length of the uploaded file.");
            }

            if (!IsValidMissingSegment(uploadSessionIdentifier, rangeFrom, rangeTo, rangeLength))
            {
                throw new InvalidRangeException("Content range is not valid - at least some part of it has already been uploaded.");
            }

            try
            {
                using (var fileStream = new FileStream(segmentFile.FullName, FileMode.CreateNew, FileAccess.Write))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // Check if all segments have been uploaded successfuly
                if (!GetMissingSegments(uploadSessionIdentifier, rangeLength).Any())
                {
                    await CreateFinalFileAsync(uploadSessionIdentifier, uploadSession.FileIdentifier);
                    uploadSession.Finished = true;
                    await SaveUploadSessionMetadataAsync(uploadSession);
                }

                return uploadSession;
            }
            catch (IOException exception)

            {
                if (!exception.Message.Contains("exists"))

                {
                    throw;
                }

                throw ExceptionHelper.ResourceAlreadyExists(segmentFile.Name, exception);
            }
            catch (Exception exception)
            {
                throw ExceptionHelper.OtherError(exception);
            }
        }

        private static DirectoryInfo GetUploadSessionDirectoryInfo(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return new DirectoryInfo(Path.Combine(UploadSessionsDirectory.FullName, uploadSessionIdentifier));
        }

        private static FileInfo GetUploadSessionMetadataFileInfo(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return new FileInfo(Path.Combine(GetUploadSessionDirectoryInfo(uploadSessionIdentifier).FullName, "metadata.json"));
        }

        private static FileInfo GetSegmentFileInfo(
            string uploadSessionIdentifier,
            long rangeFrom,
            long rangeTo)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            var uploadSessionDirectory = GetUploadSessionDirectoryInfo(uploadSessionIdentifier);

            return new FileInfo(Path.Combine(uploadSessionDirectory.FullName, string.Join("-", "segment", rangeFrom, rangeTo)));
        }

        private static bool IsValidMissingSegment(
            string uploadSessionIdentifier,
            long rangeFrom,
            long rangeTo,
            long rangeLength)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return GetMissingSegments(uploadSessionIdentifier, rangeLength)
                .Any(missingSegment =>
                    missingSegment.From <= rangeFrom
                 && missingSegment.To >= rangeTo);
        }

        private static IReadOnlyList<Range> GetUploadedSegments(
            string uploadSessionIdentifier)
        {
            var uploadSessionDirectory = GetUploadSessionDirectoryInfo(uploadSessionIdentifier);

            var segmentFiles = uploadSessionDirectory.GetFiles("segment-*");

            return (from segmentFile in segmentFiles
                    let nameSplit = segmentFile.Name.Split('-')
                    let range = new Range(long.Parse(nameSplit[1], CultureInfo.InvariantCulture), long.Parse(nameSplit[2], CultureInfo.InvariantCulture))
                    orderby range.From
                    select range)
                   .ToList();
        }

        private static IReadOnlyList<Range> GetMissingSegments(
            string uploadSessionIdentifier,
            long totalLength)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            var uploadedSegments = GetUploadedSegments(uploadSessionIdentifier);

            long current = 0;
            var missingSegments = new List<Range>();

            foreach (var uploadedSegment in uploadedSegments)
            {
                long segmentLength = uploadedSegment.From - current;

                if (segmentLength > 0)
                {
                    missingSegments.Add(new Range(current, uploadedSegment.From));
                }

                current = uploadedSegment.To + 1;
            }

            if (current < totalLength - 1)
            {
                missingSegments.Add(new Range(current, totalLength));
            }

            return missingSegments;
        }

        private async Task<UploadSession> GetInternalAsync(string uploadSessionIdentifier, bool throwExceptionOnNotFound = true)
        {
            var uploadSession = await GetAsync(uploadSessionIdentifier) as UploadSession;

            if (throwExceptionOnNotFound && uploadSession == null)
            {
                throw new ResourceNotFoundException();
            }

            if (uploadSession.ExpirationDateTime < DateTimeOffset.UtcNow)
            {
                throw new UploadSessionExpiredException();
            }

            return uploadSession;
        }

        private static async Task<UploadSession> ReadUploadSessionAsync(FileInfo uploadSessionFile)
        {
            UploadSession uploadSession = null;

            using (var reader = new StreamReader(uploadSessionFile.FullName, Encoding.UTF8))
            {
                string metadataString = await reader.ReadToEndAsync();
                uploadSession = JsonConvert.DeserializeObject<UploadSession>(metadataString);
            }

            return uploadSession;
        }

        private static async Task SaveUploadSessionMetadataAsync(IUploadSession uploadSession)
        {
            var uploadSessionFile = GetUploadSessionMetadataFileInfo(uploadSession.Id);
            var uploadSessionJson = JsonConvert.SerializeObject(uploadSession);
            var uploadSessionJsonBytes = Encoding.UTF8.GetBytes(uploadSessionJson);

            using (var fileStream = new FileStream(uploadSessionFile.FullName, FileMode.Create, FileAccess.Write))
            {
                await fileStream.WriteAsync(uploadSessionJsonBytes, 0, uploadSessionJsonBytes.Length);
            }
        }

        private static async Task CreateFinalFileAsync(string uploadSessionIdentifier, string fileIdentifier)
        {
            var segmentFiles = GetUploadSessionDirectoryInfo(uploadSessionIdentifier).EnumerateFiles();

            using (var fileStream = new FileStream(FileSystemFileDataAccess.GetFileBinaryFileInfo(fileIdentifier).FullName, FileMode.CreateNew, FileAccess.Write))
            {
                foreach (var segmentFile in segmentFiles)
                {
                    using (var segmentFileReader = new FileStream(segmentFile.FullName, FileMode.Open, FileAccess.Read))
                    {
                        await segmentFileReader.CopyToAsync(fileStream);
                    }
                }
            }
        }
    }
}