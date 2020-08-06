// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the IpBlocks table in the database.
    /// </summary>
    public partial class IpBlock
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public IpBlock()
        {
            this.IpAddresses = new HashSet<IpAddress>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        
        public string CIDR { get; set; }
        [JsonIgnore]
        public long IpConfigId { get; set; }

        [JsonIgnore]
        public virtual ICollection<IpAddress> IpAddresses { get; set; }
        [JsonIgnore]
        public virtual IpConfig IpConfig { get; set; }
    }
}