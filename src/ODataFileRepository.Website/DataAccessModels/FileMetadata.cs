using ODataFileRepository.Website.DataAccessModels.Contracts;

namespace ODataFileRepository.Website.DataAccessModels
{
    public class FileMetadata : IFileMetadata
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string MediaType { get; set; }
    }
}