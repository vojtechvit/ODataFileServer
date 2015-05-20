using ODataFileRepository.Website.DomainModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IFileDataAccess
    {
        Task<IFileMetadata> CreateAsync(string fullName, string mediaType, Stream stream);

        Task<bool> ExistsAsync(string fullName);

        Task<IReadOnlyList<IFileMetadata>> GetAllAsync();

        Task<IFileMetadata> GetMetadataAsync(string fullName);

        Task<StreamWithMetadata> GetStreamAsync(string fullName);

        Task UpdateMetadataAsync(IFileMetadata metadata);

        Task UpdateStreamAsync(string fullName, string mediaType, Stream stream);

        Task DeleteAsync(string fullName);
    }
}