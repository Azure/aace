using System.Collections.Generic;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomerMeterErrorDetails
    {
        public string Message { get; set; }

        public string Target { get; set; }
    }

    public class CustomMeteringBadRequestResult : CustomMeteringRequestResult
    {
        public string Code { get; set; }
        public IEnumerable<CustomerMeterErrorDetails> Details { get; set; }
        public string Message { get; set; }

        public string Target { get; set; }
    }
}
