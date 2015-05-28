using ODataFileRepository.Website.DataAccessModels.Contracts;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;

namespace ODataFileRepository.Website.DataAccess.Contracts
{
    public interface IUploadSessionDataAccess
    {
        Task<IUploadSession> CreateAsync(
            string uploadSessionIdentifier,
            string fileIdentifier,
            string fileName);

        Task<bool> ExistsAsync(string uploadSessionIdentifier);

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification = "Represents an operation, not a property of this class.")]
        Task<IReadOnlyList<IUploadSession>> GetAllAsync();

        Task<IUploadSession> GetAsync(string uploadSessionIdentifier);

        Task<IUploadSession> UploadSegmentAsync(
            string uploadSessionIdentifier,
            string mediaType,
            long rangeFrom,
            long rangeTo,
            long rangeLength,
            Stream stream);

        Task DeleteAsync(string uploadSessionIdentifier);
    }
}