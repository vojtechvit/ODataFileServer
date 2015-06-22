using System;
using System.Runtime.Serialization;

namespace ODataFileRepository.Website.DataAccess.Exceptions
{
    [Serializable]
    public class UploadSessionExpiredException : DataAccessException
    {
        public UploadSessionExpiredException()
        {
        }

        public UploadSessionExpiredException(string message) : base(message)
        {
        }

        public UploadSessionExpiredException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UploadSessionExpiredException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}