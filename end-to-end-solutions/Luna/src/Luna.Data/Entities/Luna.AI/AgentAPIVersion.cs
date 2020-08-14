using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    [Table("agent_apiversions")]
    public class AgentAPIVersion
    {
        public AgentAPIVersion()
        {
        }

        public string VersionName { get; set; }
        public string DeploymentName { get; set; }

        public string ProductName { get; set; }

        public string VersionSourceType { get; set; }

        public string ProjectFileUrl { get; set; }

        public Guid AgentId { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        public Guid SubscriptionId { get; set; }

        public Guid PublisherId { get; set; }
    }
}
