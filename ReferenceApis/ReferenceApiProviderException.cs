using System;

namespace ReferenceApis
{
    public class ReferenceApiProviderException : Exception
    {
        public ReferenceApiProviderException(string message)
            : base(message) { }

        public ReferenceApiProviderException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}
