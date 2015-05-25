using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IFileVersionDataAccess
    {
        Task<bool> ExistsAsync(string fileIdentifier, string fileVersionIdentifier);
        
        Task<IReadOnlyList<IFileVersionMetadata>> GetAllAsync(string fileIdentifier);

        Task<IFileVersionMetadata> GetCurrentAsync(string fileIdentifier);

        Task SetCurrentAsync(string fileIdentifier, string fileVersionIdentifier);

        Task<IFileVersionMetadata> GetAsync(string fileIdentifier, string fileVersionIdentifier);

        Task<LazyServiceStream> GetStreamAsync(string fileIdentifier, string fileVersionIdentifier);
    }
}