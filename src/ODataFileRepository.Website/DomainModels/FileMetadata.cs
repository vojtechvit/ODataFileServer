using ODataFileRepository.Website.DomainModels.Contracts;

namespace ODataFileRepository.Website.DomainModels
{
    public class FileMetadata : IFileMetadata
    {
        public string FullName { get; set; }

        public string MediaType { get; set; }
    }
}