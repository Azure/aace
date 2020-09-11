using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class AIAgent
    {
        public AIAgent()
        {

        }

        public void Copy(AIAgent agent)
        {
            IsSaaSAgent = agent.IsSaaSAgent;
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public Guid AgentId { get; set; }

        [JsonIgnore]
        public string AgentKeySecretName { get; set; }

        [NotMapped]
        public string AgentKey { get; set; }

        public string CreatedBy { get; set; }

        public DateTime LastHeartbeatReportedTime { get; set; }

        public DateTime CreatedTime { get; set; }

        public bool IsSaaSAgent { get; set; }
    }
}
