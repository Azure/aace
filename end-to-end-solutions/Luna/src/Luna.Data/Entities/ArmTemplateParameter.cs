using Luna.Data.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the armTemplateParameters table in the database.
    /// </summary>
    public partial class ArmTemplateParameter
    {
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="armTemplateParameter">The object to be copied.</param>
        public void Copy(ArmTemplateParameter armTemplateParameter)
        {
            this.Type = armTemplateParameter.Type;
            this.Value = armTemplateParameter.Value;
        }
        
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string Name { get; set; }
        
        public string Type { get; set; }
        
        public string Value { get; set; }
    
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArmTemplateArmTemplateParameter> ArmTemplateArmTemplateParameters { get; set; }
    }
}