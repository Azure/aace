// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the IpConfigs table in the database.
    /// </summary>
    public partial class IpConfig
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public IpConfig()
        {
            this.IpBlock = new HashSet<IpBlock>();
        }

        [JsonIgnore]
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public int IPsPerSub { get; set; }
        
        [NotMapped]
        public List<string> IpBlocks { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }

        [JsonIgnore]
        public virtual ICollection<IpBlock> IpBlock { get; set; }
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
    }
}