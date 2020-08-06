// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Luna.Data.Enums;

using System.Text.Json;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    /// <summary>
    /// Entity class that maps to the subcriptions table in the database.
    /// </summary>
    public partial class Subscription
    {
        /// <summary>
        /// Constructs the EF Core collection navigation properties.
        /// </summary>
        public Subscription()
        {
            this.IpAddresses = new HashSet<IpAddress>();
        }

        /// <summary>
        /// Copies all non-EF Core values.
        /// </summary>
        /// <param name="subscription">The object to be copied.</param>
        public void Copy(Subscription subscription)
        {

        }
    
        [Key]
        public Guid SubscriptionId { get; set; }
        
        public string Name { get; set; }
        
        public string PublisherId { get; set; }
        [JsonIgnore]
        public long OfferId { get; set; }
        [JsonIgnore]
        public long PlanId { get; set; }
        
        public int Quantity { get; set; }
        
        public Guid BeneficiaryTenantId { get; set; }
        
        public Guid PurchaserTenantId { get; set; }
        
        public string Status { get; set; }
        public bool? IsTest { get; set; }
        
        public int AllowedCustomerOperationsMask { get; set; } = (int) AllowedCustomerOperation.Read;
        
        public string SessionMode { get; set; } = nameof(SubscriptionSessionMode.None);
        
        public string SandboxType { get; set; } = nameof(SubscriptionSandboxType.None);
        public bool? IsFreeTrial { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ActivatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        public DateTime? LastSuspendedTime { get; set; }
        public DateTime? UnsubscribedTime { get; set; }
        public DateTime? DataDeletedTime { get; set; }
        public Guid? OperationId { get; set; }
        
        public string DeploymentName { get; set; }
        
        public Guid DeploymentId { get; set; }
        public string ResourceGroup { get; set; }

        
        public string Owner { get; set; }

        [NotMapped]
        public string OfferName { get; set; }

        [NotMapped]
        public string PlanName { get; set; }

        public string ActivatedBy { get; set; }

        [JsonIgnore]
        public string LastException { get; set; }

        public string ProvisioningStatus { get; set; }

        [JsonIgnore]
        public string ProvisioningType { get; set; }

        [JsonIgnore]
        public int RetryCount { get; set; }

        public string EntryPointUrl { get; set; }

        [NotMapped]
        public List<SubscriptionParameter> InputParameters { get; set; }

        [JsonIgnore]
        public virtual ICollection<IpAddress> IpAddresses { get; set; }
        [JsonIgnore]
        public virtual Offer Offer { get; set; }
        [JsonIgnore]
        public virtual Plan Plan { get; set; }

    }
}