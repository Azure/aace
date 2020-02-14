using System;

namespace Luna.Clients.Exceptions
{
    public class LunaServerException : Exception
    {
        public LunaServerException(
            string message,
            bool isRetryable = default,
            Exception innerException = default) : base(message, innerException)
        {
            this.IsRetryable = isRetryable;
        }

        public bool IsRetryable { get; set; }
    }
}
