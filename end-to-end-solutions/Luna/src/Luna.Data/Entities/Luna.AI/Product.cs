using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the offers table in the database.
    /// </summary>
    public partial class Product
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public Product()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="product">The object to be copied.</param>
        public void Copy(Product product)
        {
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }

        public string ProductName { get; set; }

        public string ProductType { get; set; }

        public string HostType { get; set; }

        public string Owner { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

        [JsonIgnore]
        public virtual ICollection<Deployment> Deployments { get; set; }
    }
}