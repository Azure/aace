using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Data.DataContracts.Luna.AI
{
    public class AzureMLPipeline
    {
        public string DisplayName { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public string Description { get; set; }

        public string Id { get; set; }
    }
}
