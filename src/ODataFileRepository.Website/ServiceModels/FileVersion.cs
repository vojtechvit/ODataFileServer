using ODataFileRepository.Website.DataAccessModels.Contracts;
using ODataFileRepository.Website.Infrastructure.ODataExtensions.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataFileRepository.Website.ServiceModels
{
    public class FileVersion : IFileVersionMetadata, IMediaTypeHolder
    {
        public FileVersion()
        {
        }

        public FileVersion(IFileVersionMetadata fileVersionMetadata)
        {
            if (fileVersionMetadata == null)
            {
                throw new ArgumentNullException("fileVersionMetadata");
            }

            Id = fileVersionMetadata.Id;
            Size = fileVersionMetadata.Size;
            MediaType = fileVersionMetadata.MediaType;
            UploadedDateTime = fileVersionMetadata.UploadedDateTime;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Size { get; set; }

        [NotMapped]
        public string MediaType { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset UploadedDateTime { get; set; }
    }
}