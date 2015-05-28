using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;

namespace ODataFileRepository.Website.DataAccessModels
{
    public class UploadSession : IUploadSession
    {
        public string Id { get; set; }

        public string FileIdentifier { get; set; }

        public string FileName { get; set; }

        public string FileMediaType { get; set; }

        public DateTimeOffset ExpirationDateTime { get; set; }

        public bool Finished { get; set; }
    }
}