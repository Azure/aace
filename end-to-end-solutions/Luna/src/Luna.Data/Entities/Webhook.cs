using Luna.Data.Entities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the webhooks table in lunaSqlDb.
    /// </summary>
    public partial class Webhook
    {
        public Webhook()
        {
            this.SubscribeWebhookNav = new HashSet<Plan>();
            this.UnsubscribeWebhookNav = new HashSet<Plan>();
            this.SuspendWebhookNav = new HashSet<Plan>();
            this.DeleteDataWebhookNav = new HashSet<Plan>();
        }
        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="armTemplate">The object to be copied.</param>
        public void Copy(Webhook webhook)
        {
            this.WebhookName = webhook.WebhookName;
            this.WebhookUrl = webhook.WebhookUrl;
        }
    
        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        
        public string WebhookName { get; set; }
        
        public string WebhookUrl { get; set; }


        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> SubscribeWebhookNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> UnsubscribeWebhookNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> SuspendWebhookNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<Plan> DeleteDataWebhookNav { get; set; }
        [JsonIgnore]
        public virtual ICollection<WebhookWebhookParameter> WebhookWebhookParameters { get; set; }
    }
}