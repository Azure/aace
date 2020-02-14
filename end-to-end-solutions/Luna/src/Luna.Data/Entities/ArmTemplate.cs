using Luna.Data.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the armTemplates table in the database.
    /// </summary>
    public partial class ArmTemplate
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public ArmTemplate()
        {
            this.SubscribeArmTemplateNav = new HashSet<Plan>();
            this.UnsubscribeArmTemplateNav = new HashSet<Plan>();
            this.SuspendArmTemplateNav = new HashSet<Plan>();
            this.DeleteDataArmTemplateNav = new HashSet<Plan>();
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="armTemplate">The object to be copied.</param>
        public void Copy(ArmTemplate armTemplate)
        {
            this.TemplateFilePath = armTemplate.TemplateFilePath;
        }
    
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string TemplateName { get; set; }
        
        public string TemplateFilePath { get; set; }
    
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> SubscribeArmTemplateNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> UnsubscribeArmTemplateNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> SuspendArmTemplateNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> DeleteDataArmTemplateNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<ArmTemplateArmTemplateParameter> ArmTemplateArmTemplateParameters { get; set; }
    }
}