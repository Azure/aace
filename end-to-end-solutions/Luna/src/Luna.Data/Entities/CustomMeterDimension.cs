// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the customMeterDimensions table in the database.
    /// </summary>
    public partial class CustomMeterDimension
    {
        /// <summary>
        /// Copies all non-EF core values.
        /// </summary>
        /// <param name="customMeterDimension">The object to be copied.</param>
        public void Copy(CustomMeterDimension customMeterDimension)
        {
            this.MeterName = customMeterDimension.MeterName;
            this.PlanName = customMeterDimension.PlanName;
            this.MonthlyUnlimited = customMeterDimension.MonthlyUnlimited;
            this.AnnualUnlimited = customMeterDimension.AnnualUnlimited;
            this.MonthlyQuantityIncludedInBase = customMeterDimension.MonthlyQuantityIncludedInBase;
            this.AnnualQuantityIncludedInBase = customMeterDimension.AnnualQuantityIncludedInBase;
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long MeterId { get; set; }
        
        [NotMapped]
        public string MeterName { get; set; }
        [JsonIgnore]
        public long PlanId { get; set; }

        [NotMapped]
        public string PlanName { get; set; }
        public bool? MonthlyUnlimited { get; set; }
        public bool? AnnualUnlimited { get; set; }
        public int? MonthlyQuantityIncludedInBase { get; set; }
        public int? AnnualQuantityIncludedInBase { get; set; }
    
        [JsonIgnore]
        public virtual Plan Plan { get; set; }
    }
}