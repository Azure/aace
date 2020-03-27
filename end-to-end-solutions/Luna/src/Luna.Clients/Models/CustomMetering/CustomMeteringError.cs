using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomMeteringError
    {
        public string Message { get; set; }
        public string Target { get; set; }
        public CustomMeteringError[] Details { get; set; }

        public string Code { get; set; }

        public AdditionalErrorInfo AdditionalInfo { get; set; }
    }

    public class AdditionalErrorInfo
    {
        public CustomMeteringSuccessResult AcceptedMessage { get; set; }
    }
}
