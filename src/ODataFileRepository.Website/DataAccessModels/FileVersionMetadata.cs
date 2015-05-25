using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;

namespace ODataFileRepository.Website.DataAccessModels
{
    public class FileVersionMetadata : IFileVersionMetadata
    {
        public string Id { get; set; }

        public string MediaType { get; set; }

        public long Size { get; set; }

        public DateTimeOffset UploadedDateTime { get; set; }
    }
}