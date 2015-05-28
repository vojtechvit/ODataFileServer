using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;

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
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string Name { get; set; }

        [Required]
        public string MediaType { get; set; }

        [Required]
        public FileVersion CurrentVersion { get; set; }

        [Contained]
        public ICollection<FileVersion> Versions { get; set; }
    }
}