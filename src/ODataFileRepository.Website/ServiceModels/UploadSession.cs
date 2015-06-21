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
            FileIdentifier = uploadSession.FileIdentifier;
            FileName = uploadSession.FileName;
            FileMediaType = uploadSession.FileMediaType;
            FileSize = uploadSession.FileSize;
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

            FileIdentifier = uploadedFile.Id;
            FileName = uploadedFile.Name;
            FileMediaType = uploadedFile.MediaType;
            FileSize = uploadedFile.Size;
            UploadedFile = new File(uploadedFile);
        }

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FileIdentifier { get; set; }

        public string FileName { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public string FileMediaType { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public long FileSize { get; set; }

        [Required, DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public DateTimeOffset ExpirationDateTime { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]
        public File UploadedFile { get; set; }
    }
}