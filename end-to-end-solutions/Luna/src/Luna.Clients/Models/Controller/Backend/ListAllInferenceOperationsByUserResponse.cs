using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class ListAllInferenceOperationsByUserResponse
    {
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string endTimeUtc { get; set; }
        public String description { get; set; }
        public Object error { get; set; }
        public Tags tags { get; set; }
        public class Tags
        {
            public string operationId { get; set; }
            public string operationType { get; set; }
        }
        public ListAllInferenceOperationsByUserResponse()
        {
            this.tags = new Tags();
        }
    }
}
