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
    public partial class APISubscription
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public APISubscription()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="subscription">The object to be copied.</param>
        public void Copy(APISubscription subscription)
        {
            this.ProductName = subscription.ProductName;
            this.DeploymentName = subscription.DeploymentName;
            this.BaseUrl = subscription.BaseUrl;
            this.Status = subscription.Status;
        }

        [Key]
        public Guid SubscriptionId { get; set; }

        public string SubscriptionName { get; set; }

        //[JsonIgnore]
        //public long DeploymentId { get; set; }

        [NotMapped]
        public string ProductName { get; set; }

        [NotMapped]
        public string DeploymentName { get; set; }

        public string UserId { get; set; }

        public string Status { get; set; }

        public string BaseUrl { get; set; }

        [NotMapped]
        public string PrimaryKey { get; set; }

        [NotMapped]
        public string SecondaryKey { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

    }
}