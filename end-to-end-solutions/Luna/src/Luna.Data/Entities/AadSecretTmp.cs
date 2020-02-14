using System;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the aadSecretTmps table in the database.
    /// </summary>
    public partial class AadSecretTmp
    {
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="aadSecretTmp">The object to copy.</param>
        public void Copy(AadSecretTmp aadSecretTmp)
        {
            this.Name = aadSecretTmp.Name;
            this.TenantId = aadSecretTmp.TenantId;
            this.ApplicationId = aadSecretTmp.ApplicationId;
            this.ClientSecret = aadSecretTmp.ClientSecret;
        }
        
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string Name { get; set; }
        
        public Guid TenantId { get; set; }
        
        public Guid ApplicationId { get; set; }
        
        public string ClientSecret { get; set; }
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
    }
}