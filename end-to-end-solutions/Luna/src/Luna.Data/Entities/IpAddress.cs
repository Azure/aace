// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the ipAddresses table in the database.
    /// </summary>
    public partial class IpAddress
    {
        [JsonIgnore]
        public long Id { get; set; }
        
        public string Value { get; set; }
        
        public bool IsAvailable { get; set; }
        [JsonIgnore]
        public long IpBlockId { get; set; }
        public Guid? SubscriptionId { get; set; }
    
        [JsonIgnore]
        public virtual IpBlock IpBlock { get; set; }
        [JsonIgnore]
        public virtual Subscription Subscription { get; set; }
    }
}