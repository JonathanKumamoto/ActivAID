using System;
using System.Runtime.Serialization;

namespace ActivAID
{
    [Serializable]
    internal class NoFileMatchException : Exception
    {
        public NoFileMatchException()
        {
        }

        public NoFileMatchException(string message) : base(message)
        {
        }

        public NoFileMatchException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoFileMatchException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}