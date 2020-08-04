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
    public partial class AMLWorkspace
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public AMLWorkspace()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="workspace">The object to be copied.</param>
        public void Copy(AMLWorkspace workspace)
        {
            Region = workspace.Region;
            ResourceId = workspace.ResourceId;
            AADApplicationId = workspace.AADApplicationId;
            AADApplicationSecretName = workspace.AADApplicationSecretName;
            AADTenantId = workspace.AADTenantId;
        }
    
        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string WorkspaceName { get; set; }

        public string Region { get; set; }

        public string ResourceId { get; set; }
        
        public Guid AADApplicationId { get; set; }

        [NotMapped]
        public string AADApplicationSecrets { get; set; }

        [JsonIgnore]
        public string AADApplicationSecretName { get; set; }

        public Guid AADTenantId { get; set; }
    }
}