using System;
using System.Runtime.Serialization;

namespace ODataFileRepository.Website.DataAccess.Exceptions
{
    [Serializable]
    public class InvalidMediaTypeException : DataAccessException
    {
        public InvalidMediaTypeException()
        {
        }

        public InvalidMediaTypeException(string message) : base(message)
        {
        }

        public InvalidMediaTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidMediaTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}