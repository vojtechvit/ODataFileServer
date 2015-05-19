using ODataFileRepository.Website.DomainModels.Contracts;
using System.ComponentModel.DataAnnotations;

namespace ODataFileRepository.Website.ServiceModels
{
    public class File : IFileMetadata
    {
        public File()
        {
        }

        public File(IFileMetadata fileMetadata)
        {
            FullName = fileMetadata.FullName;
            MediaType = fileMetadata.MediaType;
        }

        [Key]
        public string FullName { get; set; }

        [Required]
        // See 3.1.1.1 http://www.ietf.org/rfc/rfc7231.txt
        [RegularExpression("^[!#$%&'*+-.^_`|~0-9a-zA-Z]+/[!#$%&'*+-.^_`|~0-9a-zA-Z]+$", ErrorMessage = "Invalid media type")]
        public string MediaType { get; set; }
    }
}