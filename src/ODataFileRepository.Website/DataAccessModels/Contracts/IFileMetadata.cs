namespace ODataFileRepository.Website.DataAccessModels.Contracts
{
    public interface IFileMetadata
    {
        string Id { get; }

        string Name { get; }

        string MediaType { get; }

        long Size { get; }
    }
}