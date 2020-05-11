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
    public partial class APIVersion
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public APIVersion()
        {
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="version">The object to be copied.</param>
        public void Copy(APIVersion version)
        {
            this.ProductName = version.ProductName;
            this.DeploymentName = version.DeploymentName;
            this.RealTimePredictAPI = version.RealTimePredictAPI;
            this.BatchInferenceAPI = version.BatchInferenceAPI;
            this.TrainModelAPI = version.TrainModelAPI;
            this.DeployModelAPI = version.DeployModelAPI;
            this.AuthenticationType = version.AuthenticationType;
            this.AuthenticationKey = version.AuthenticationKey;
        }

        public string GetVersionIdFormat()
        {
            return VersionName.Replace(".", "-");
        }

        [Key]
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long DeploymentId { get; set; }
        [NotMapped]
        public string ProductName { get; set; }
        [NotMapped]
        public string DeploymentName { get; set; }

        public string VersionName { get; set; }

        public string RealTimePredictAPI { get; set; }

        public string TrainModelAPI { get; set; }

        public string BatchInferenceAPI { get; set; }

        public string DeployModelAPI { get; set; }

        public string AuthenticationType { get; set; }

        public string AuthenticationKey { get; set; }

        [JsonIgnore]
        public long AMLWorkspaceId { get; set; }

        [NotMapped]
        public string AMLWorkspaceName { get; set; }

        public string AdvancedSettings { get; set; }

        public DateTime CreatedTime { get; set; }

        public DateTime LastUpdatedTime { get; set; }

    }
}