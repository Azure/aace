// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the restrictedUsers table in the database.
    /// </summary>
    public partial class RestrictedUser
    {
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="restrictedUser">The object to be copied.</param>
        public void Copy(RestrictedUser restrictedUser)
        {
            this.TenantId = restrictedUser.TenantId;
            this.Description = restrictedUser.Description;
        }

        public long Id { get; set; }
        [JsonIgnore]
        public long PlanId { get; set; }
        
        public Guid TenantId { get; set; }
        
        public string Description { get; set; }
    
        [JsonIgnore]
        public virtual Plan Plan { get; set; }
    }
}