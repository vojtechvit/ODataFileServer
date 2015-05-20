using ODataFileRepository.Website.DomainModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IFileDataAccess
    {
        Task<IFileMetadata> CreateAsync(string fullName, string mediaType, Stream stream);

        Task<bool> ExistsAsync(string fullName);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Represents an operation, not a property of this class.")]
        Task<IReadOnlyList<IFileMetadata>> GetAllAsync();

        Task<IFileMetadata> GetMetadataAsync(string fullName);

        Task<LazyServiceStream> GetStreamAsync(string fullName);

        Task UpdateMetadataAsync(IFileMetadata metadata);

        Task UpdateStreamAsync(string fullName, string mediaType, Stream stream);

        Task DeleteAsync(string fullName);
    }
}