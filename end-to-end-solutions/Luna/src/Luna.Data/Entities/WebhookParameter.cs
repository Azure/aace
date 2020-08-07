// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Luna.Data.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the webhookParameters table in lunaSqlDb.
    /// </summary>
    public partial class WebhookParameter
    {
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="armTemplateParameter">The object to be copied.</param>
        public void Copy(WebhookParameter webhookParameter)
        {
            this.Value = webhookParameter.Value;
        }
        
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string Name { get; set; }
        
        public string Value { get; set; }
    
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual ICollection<WebhookWebhookParameter> WebhookWebhookParameters { get; set; }
    }
}