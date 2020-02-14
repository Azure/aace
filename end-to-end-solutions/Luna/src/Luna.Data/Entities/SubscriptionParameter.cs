using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public class SubscriptionParameter
    {

        [JsonIgnore]
        [Key]
        public long Id { get; set; }

        public Guid SubscriptionId { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Value { get; set; }

        public SubscriptionParameter()
        {
        }

        public void Copy(SubscriptionParameter param)
        {
            this.SubscriptionId = param.SubscriptionId;
            this.Name = param.Name;
            this.Type = param.Type;
            this.Value = param.Value;
        }
    }
}
