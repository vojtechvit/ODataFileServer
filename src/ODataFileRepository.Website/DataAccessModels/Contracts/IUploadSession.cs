using System;

namespace ODataFileRepository.Website.DataAccessModels.Contracts
{
    public interface IUploadSession
    {
        string Id { get; }

        string FileName { get; }
        
        DateTimeOffset ExpirationDateTime { get; }
        
        string FileVersionIdentifier { get; }
    }
}