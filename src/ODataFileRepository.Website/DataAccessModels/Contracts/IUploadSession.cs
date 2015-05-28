using System;

namespace ODataFileRepository.Website.DataAccessModels.Contracts
{
    public interface IUploadSession
    {
        string Id { get; }

        string FileIdentifier { get; }

        string FileName { get; }

        string FileMediaType { get; }

        DateTimeOffset ExpirationDateTime { get; }

        bool Finished { get; }
    }
}