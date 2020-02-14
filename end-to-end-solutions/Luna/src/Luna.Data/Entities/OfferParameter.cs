using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{   /// <summary>
    /// Entity class that maps to the offerParameters table in the database.
    /// </summary>
    public partial class OfferParameter
    {
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="offerParameter">The object to be copied.</param>
        public void Copy(OfferParameter offerParameter)
        {
            this.ParameterName = offerParameter.ParameterName;
            this.DisplayName = offerParameter.DisplayName;
            this.Description = offerParameter.Description;
            this.ValueType = offerParameter.ValueType;
            this.FromList = offerParameter.FromList;
            this.ValueList = offerParameter.ValueList;
            this.Maximum = offerParameter.Maximum;
            this.Minimum = offerParameter.Minimum;
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string ParameterName { get; set; }
        
        public string DisplayName { get; set; }
        
        public string Description { get; set; }
        
        public string ValueType { get; set; }
        
        public bool FromList { get; set; }
        
        public string ValueList { get; set; }
        public long? Maximum { get; set; }
        public long? Minimum { get; set; }
    
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
    }
}