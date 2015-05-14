namespace ODataFileRepository.Website.Models.Contracts
{
    public interface IFileMetadata
    {
        string FullName { get; }

        string MediaType { get; }
    }
}