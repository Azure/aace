// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the plans table in the database.
    /// </summary>
    public partial class Plan
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public Plan()
        {
            this.CustomMeterDimensions = new HashSet<CustomMeterDimension>();
            this.RestrictedUsers = new HashSet<RestrictedUser>();
            this.Subscriptions = new HashSet<Subscription>();
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="plan">The object to be copied.</param>
        public void Copy(Plan plan)
        {
            this.PlanName = plan.PlanName;
            this.DataRetentionInDays = plan.DataRetentionInDays;
            this.SubscribeArmTemplateName = plan.SubscribeArmTemplateName;
            this.UnsubscribeArmTemplateName = plan.UnsubscribeArmTemplateName;
            this.SuspendArmTemplateName = plan.SuspendArmTemplateName;
            this.DeleteDataArmTemplateName = plan.DeleteDataArmTemplateName;
            this.SubscribeWebhookName = plan.SubscribeWebhookName;
            this.UnsubscribeWebhookName = plan.UnsubscribeWebhookName;
            this.SuspendWebhookName = plan.SuspendWebhookName;
            this.DeleteDataWebhookName = plan.DeleteDataWebhookName;
            this.PriceModel = plan.PriceModel;
            this.MonthlyBase = plan.MonthlyBase;
            this.AnnualBase = plan.AnnualBase;
            this.PrivatePlan = plan.PrivatePlan;
        }

        [JsonIgnore]
        public long Id { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }

        public string PlanName { get; set; }

        public int DataRetentionInDays { get; set; }
        [JsonIgnore]
        public long? SubscribeArmTemplateId { get; set; }

        [NotMapped]
        public string SubscribeArmTemplateName { get; set; }

        [JsonIgnore]
        public long? SubscribeWebhookId { get; set; }

        [NotMapped]
        public string SubscribeWebhookName { get; set; }

        [JsonIgnore]
        public long? UnsubscribeArmTemplateId { get; set; }
        
        [NotMapped]
        public string UnsubscribeArmTemplateName { get; set; }

        [JsonIgnore]
        public long? UnsubscribeWebhookId { get; set; }

        [NotMapped]
        public string UnsubscribeWebhookName { get; set; }

        [JsonIgnore]
        public long? SuspendArmTemplateId { get; set; }
        
        [NotMapped]
        public string SuspendArmTemplateName { get; set; }

        [JsonIgnore]
        public long? SuspendWebhookId { get; set; }

        [NotMapped]
        public string SuspendWebhookName { get; set; }

        [JsonIgnore]
        public long? DeleteDataArmTemplateId { get; set; }
        
        [NotMapped]
        public string DeleteDataArmTemplateName { get; set; }

        [JsonIgnore]
        public long? DeleteDataWebhookId { get; set; }

        [NotMapped]
        public string DeleteDataWebhookName { get; set; }

        public string PriceModel { get; set; }
        public double? MonthlyBase { get; set; }
        public double? AnnualBase { get; set; }
       
        public bool PrivatePlan { get; set; }
    
        [JsonIgnore]
        public virtual ArmTemplate SubscribeArmTemplate { get; set; }
        [JsonIgnore]
        public virtual ArmTemplate UnsubscribeArmTemplate { get; set; }
        [JsonIgnore]
        public virtual ArmTemplate SuspendArmTemplate { get; set; }
        [JsonIgnore]
        public virtual ArmTemplate DeleteDataArmTemplate { get; set; }

        [JsonIgnore]
        public virtual Webhook SubscribeWebhook { get; set; }
        [JsonIgnore]
        public virtual Webhook UnsubscribeWebhook { get; set; }
        [JsonIgnore]
        public virtual Webhook SuspendWebhook { get; set; }
        [JsonIgnore]
        public virtual Webhook DeleteDataWebhook { get; set; }
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual ICollection<CustomMeterDimension> CustomMeterDimensions { get; set; }
        [JsonIgnore]
        public virtual ICollection<RestrictedUser> RestrictedUsers { get; set; }
        [JsonIgnore]
        public virtual ICollection<Subscription> Subscriptions { get; set; }
    }
}
