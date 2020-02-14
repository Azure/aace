using System;
using System.Runtime.Serialization;

namespace Luna.Services.Marketplace
{
    [Serializable]
    internal class FulfillmentManagerException : Exception
    {
        public FulfillmentManagerException()
        {
        }

        public FulfillmentManagerException(FulfillmentManagerOperationResult fulfillmentManagerOperationResult)
        {
            this.OperationResult = fulfillmentManagerOperationResult;
        }

        public FulfillmentManagerException(string message)
            : base(message)
        {
        }

        public FulfillmentManagerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected FulfillmentManagerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public FulfillmentManagerOperationResult OperationResult { get; set; }
    }
}