using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    [Table("agent_subscriptions")]
    public class AgentSubscription
    {
        public AgentSubscription()
        {
        }

        [JsonPropertyName("SubscriptionId")]
        public Guid SubscriptionId { get; set; }

        [JsonPropertyName("DeploymentName")]
        public string DeploymentName { get; set; }

        [JsonPropertyName("ProductName")]
        public string ProductName { get; set; }

        [JsonPropertyName("ProductType")]
        public string ProductType { get; set; }

        [JsonPropertyName("UserId")]
        public string UserId { get; set; }

        [JsonPropertyName("SubscriptionName")]
        public string SubscriptionName { get; set; }

        [JsonPropertyName("Status")]
        public string Status { get; set; }

        [JsonPropertyName("HostType")]
        public string HostType { get; set; }

        [JsonPropertyName("CreatedTime")]
        public DateTime CreatedTime { get; set; }

        [JsonPropertyName("BaseUrl")]
        public string BaseUrl { get; set; }

        [JsonPropertyName("PrimaryKeySecretName")]
        [JsonIgnore]
        public string PrimaryKeySecretName { get; set; }

        [JsonPropertyName("SecondaryKeySecretName")]
        [JsonIgnore]
        public string SecondaryKeySecretName { get; set; }

        [JsonPropertyName("AgentId")]
        public Guid? AgentId { get; set; }

        [JsonPropertyName("PublisherId")]
        public Guid PublisherId { get; set; }

        [JsonPropertyName("OfferName")]
        public string OfferName { get; set; }

        [JsonPropertyName("PlanName")]
        public string PlanName { get; set; }

        [NotMapped]
        public string PrimaryKey { get; set; }

        [NotMapped]
        public string SecondaryKey { get; set; }
    }
}
