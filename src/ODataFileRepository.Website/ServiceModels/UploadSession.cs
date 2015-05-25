using ODataFileRepository.Website.DataAccessModels.Contracts;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ODataFileRepository.Website.ServiceModels
{
    public class UploadSession
    {
        public UploadSession()
        {
        }

        public UploadSession(IUploadSession uploadSession)
        {
            if (uploadSession == null)
            {
                throw new ArgumentNullException("uploadSession");
            }

            Id = uploadSession.Id;
            FileName = uploadSession.FileName;
            ExpirationDateTime = uploadSession.ExpirationDateTime;
        }

        public UploadSession(
            IUploadSession uploadSession,
            IFileMetadata uploadedFile)
            : this(uploadSession)
        {
            if (uploadedFile == null)
            {
                throw new ArgumentNullException("uploadedFile");
            }

            UploadedFile = new File(uploadedFile);
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        public string FileName { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset ExpirationDateTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public File UploadedFile { get; set; }
    }
}