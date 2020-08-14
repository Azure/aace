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

        public Guid SubscriptionId { get; set; }

        public string DeploymentName { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string UserId { get; set; }

        public string SubscriptionName { get; set; }

        public string Status { get; set; }

        public string HostType { get; set; }

        public DateTime CreatedTime { get; set; }

        public string BaseUrl { get; set; }

        public string PrimaryKey { get; set; }

        public string SecondaryKey { get; set; }

        public Guid? AgentId { get; set; }

        public Guid PublisherId { get; set; }
    }
}
