using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IFileDataAccess
    {
        Task<IFileMetadata> CreateAsync(string identifier, string name, string mediaType, Stream stream);

        Task<bool> ExistsAsync(string identifier);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Represents an operation, not a property of this class.")]
        Task<IReadOnlyList<IFileMetadata>> GetAllAsync();

        Task<IFileMetadata> GetAsync(string identifier);

        Task<LazyMediaStream> GetStreamAsync(string identifier);

        Task UpdateAsync(IFileMetadata metadata);

        Task UpdateStreamAsync(string identifier, string mediaType, Stream stream);

        Task DeleteAsync(string identifier);
    }
}