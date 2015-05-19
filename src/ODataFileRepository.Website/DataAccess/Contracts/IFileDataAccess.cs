using ODataFileRepository.Website.DomainModels.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IFileDataAccess
    {
        Task<IFileMetadata> CreateAsync(string fullName, string mediaType, Stream stream);

        Task<bool> ExistsAsync(string fullName);

        Task<IFileMetadata> GetMetadataAsync(string fullName);

        Task<Stream> GetStreamAsync(string fullName);

        Task<IReadOnlyList<IFileMetadata>> GetAllAsync();

        Task UpdateMetadataAsync(IFileMetadata file);

        Task DeleteAsync(string fullName);
    }
}