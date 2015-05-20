using System;
using System.Runtime.Serialization;

namespace ODataFileRepository.Website.DataAccess.Exceptions
{
    [Serializable]
    public class ResourceAlreadyExistsException : DataAccessException
    {
        public ResourceAlreadyExistsException()
        {
        }

        public ResourceAlreadyExistsException(string message) : base(message)
        {
        }

        public ResourceAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ResourceAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}