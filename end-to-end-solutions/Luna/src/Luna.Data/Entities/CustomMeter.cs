using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        }

        /// <summary>
        /// Copies all non-EF core values.
        /// </summary>
        /// <param name="customMeter">The object to be copied.</param>
        public void Copy(CustomMeter customMeter)
        {
            this.MeterName = customMeter.MeterName;
            this.OfferName = customMeter.OfferName;
            this.TelemetryDataConnectorName = customMeter.TelemetryDataConnectorName;
            this.TelemetryQuery = customMeter.TelemetryQuery;
        }
    
        [JsonIgnore]
        public long Id { get; set; }

        [NotMapped]
        public string OfferName { get; set; }

        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string MeterName { get; set; }

        [JsonIgnore]
        public long TelemetryDataConnectorId { get; set; }

        [NotMapped]
        public string TelemetryDataConnectorName { get; set; }

        public string TelemetryQuery { get; set; }

        [JsonIgnore]
        public virtual TelemetryDataConnector TelemetryDataConnector { get; set; }
    }
}