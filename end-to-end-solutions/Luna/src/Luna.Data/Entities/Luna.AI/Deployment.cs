// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class Deployment
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public Deployment()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="deployment">The object to be copied.</param>
        public void Copy(Deployment deployment)
        {
            this.ProductName = deployment.ProductName;
            this.Description = deployment.Description;
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        [JsonIgnore]
        public long ProductId { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        public string DeploymentName { get; set; }

        public string Description { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public virtual Product Product { get; set; }

        [JsonIgnore]
        public virtual ICollection<APIVersion> Versions { get; set; }
    }
}