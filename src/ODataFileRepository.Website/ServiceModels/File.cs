using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataFileRepository.Website.ServiceModels
{
    public class File : IFileMetadata
    {
        public File()
        {
        }

        public File(IFileMetadata fileMetadata)
        {
            if (fileMetadata == null)
            {
                throw new ArgumentNullException("fileMetadata");
            }

            Id = fileMetadata.Id;
            Name = fileMetadata.Name;
            MediaType = fileMetadata.MediaType;
            Size = fileMetadata.Size;
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Name { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string MediaType { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long Size { get; set; }
    }
}