using System.ComponentModel.DataAnnotations;

namespace ODataFileRepository.Website.Models
{
    public class File
    {
        [Key]
        public string FullName { get; set; }

        public string MediaType { get; set; }
    }
}