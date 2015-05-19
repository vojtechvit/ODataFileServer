namespace ODataFileRepository.Website.DomainModels.Contracts
{
    public interface IFileMetadata
    {
        string FullName { get; }

        string MediaType { get; }
    }
}