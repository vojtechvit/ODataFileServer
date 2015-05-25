using Newtonsoft.Json;
using ODataFileRepository.Website.DataAccess.Contracts;
using ODataFileRepository.Website.DataAccess.Exceptions;
using ODataFileRepository.Website.DataAccessModels;
using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System;
using System.Collections;
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
        private static DirectoryInfo _appDataDirectory = GetAppDataDirectory();

        public Task<IUploadSession> CreateAsync(
            string uploadSessionIdentifier, 
            string fileIdentifier, 
            string fileName)
        {
            throw new NotImplementedException();
        }

        public Task<IUploadSession> DeleteAsync(string uploadSessionIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string uploadSessionIdentifier)
        {
            return Task.FromResult(GetUploadSessionMetadataFileInfo(uploadSessionIdentifier).Exists);
        }

        public Task<IReadOnlyList<IUploadSession>> GetAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IUploadSession> GetAsync(string uploadSessionIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<IUploadSession> UploadSegmentAsync(string uploadSessionIdentifier, string mediaType, long rangeFrom, long rangeTo, long rangeLength, Stream stream)
        {
            throw new NotImplementedException();
        }

        private static DirectoryInfo GetAppDataDirectory()
        {
            return new DirectoryInfo(AppDomain.CurrentDomain.GetData("DataDirectory").ToString());
        }

        private static DirectoryInfo GetUploadSessionDirectoryInfo(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return new DirectoryInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "uploadsessions", uploadSessionIdentifier));
        }

        private static FileInfo GetUploadSessionMetadataFileInfo(string uploadSessionIdentifier)
        {
            if (uploadSessionIdentifier == null)
            {
                throw new ArgumentNullException("uploadSessionIdentifier");
            }

            return new FileInfo(Path.Combine(FileSystemHelpers.AppDataDirectory.FullName, "uploadsessions", uploadSessionIdentifier, "metadata.json"));
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

            return new FileInfo(Path.Combine(uploadSessionDirectory.FullName, string.Join(" -", "segment", rangeFrom, rangeTo)));
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
                    let range = new Range(long.Parse(nameSplit[1]), long.Parse(nameSplit[2]))
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
            
            foreach(var uploadedSegment in uploadedSegments)
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
    }
}