using System;

namespace ODataFileRepository.Website.DataAccessModels.Contracts
{
    public interface IFileVersionMetadata
    {
        string Id { get; }

        long Size { get; }

        string MediaType { get; }

        DateTimeOffset UploadedDateTime { get; }
    }
}