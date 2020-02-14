using System.Collections.Generic;

namespace Luna.Clients.Models.CustomMetering
{
    public class CustomMeteringBatchSuccessResult : CustomMeteringRequestResult
    {
        public int Count { get; set; }

        public IEnumerable<CustomMeteringSuccessResult> Result { get; set; }
    }
}
