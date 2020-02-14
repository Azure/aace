using Luna.Clients.Models.Fulfillment;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomMeteringRequestResult : HttpRequestResult
    {
        public CustomMeteringRequestResult()
        {
            this.Code = "Ok";
        }

        public string Code { get; set; }
    }
}
