using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the customMeters table in the database.
    /// </summary>
    public partial class CustomMeter
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public CustomMeter()
        {
            this.CustomMeterDimensions = new HashSet<CustomMeterDimension>();
        }

        /// <summary>
        /// Copies all non-EF core values.
        /// </summary>
        /// <param name="customMeter">The object to be copied.</param>
        public void Copy(CustomMeter customMeter)
        {
            this.MeterName = customMeter.MeterName;
            this.DisplayName = customMeter.DisplayName;
            this.UnitOfMeasure = customMeter.UnitOfMeasure;
            this.PricePerUnit = customMeter.PricePerUnit;
        }
    
        [JsonIgnore]
        public long Id { get; set; }
        
        public string MeterName { get; set; }
        
        public string DisplayName { get; set; }
        
        public string UnitOfMeasure { get; set; }
        
        public double PricePerUnit { get; set; }
    
        [JsonIgnore]
        public virtual ICollection<CustomMeterDimension> CustomMeterDimensions { get; set; }
    }
}