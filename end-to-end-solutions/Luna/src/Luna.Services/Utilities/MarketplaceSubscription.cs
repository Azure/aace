using System;
using System.ComponentModel.DataAnnotations;
using Luna.Clients.Models.Fulfillment;

namespace Luna.Services.Utilities
{
    public class MarketplaceSubscription
    {
        public string OfferId { get; set; }

        public string PlanId { get; set; }

        public string PublisherId { get; set; }

        public Guid BeneficiaryTenantId { get; set; }

        public Guid PurchaserTenantId { get; set; }

        public int Quantity { get; set; }

        public StatusEnum State { get; set; }

        public Guid SubscriptionId { get; set; }

        [Display(Name = "Name")]
        public string SubscriptionName { get; set; }

        internal static MarketplaceSubscription From(SubscriptionResult subscription, StatusEnum newState)
        {
            return new MarketplaceSubscription
                       {
                           SubscriptionId = subscription.SubscriptionId,
                           OfferId = subscription.OfferId,
                           PlanId = subscription.PlanId,
                           PublisherId = subscription.PublisherId,
                           BeneficiaryTenantId = subscription.Beneficiary.TenantId,
                           PurchaserTenantId = subscription.Purchaser.TenantId,
                           Quantity = subscription.Quantity,
                           SubscriptionName = subscription.Name,
                           State = newState
                       };
        }

        internal static MarketplaceSubscription From(SubscriptionResult subscription)
        {
            return new MarketplaceSubscription
                       {
                           SubscriptionId = subscription.SubscriptionId,
                           OfferId = subscription.OfferId,
                           PlanId = subscription.PlanId,
                           Quantity = subscription.Quantity,
                           SubscriptionName = subscription.Name,
                           PublisherId = subscription.PublisherId,
                           BeneficiaryTenantId = subscription.Beneficiary.TenantId,
                           PurchaserTenantId = subscription.Purchaser.TenantId,
                           State = subscription.SaasSubscriptionStatus
                       };
        }

        internal static MarketplaceSubscription From(ResolvedSubscriptionResult subscription, StatusEnum newState)
        {
            return new MarketplaceSubscription
            {
                SubscriptionId = subscription.SubscriptionId,
                OfferId = subscription.OfferId,
                PlanId = subscription.PlanId,
                Quantity = subscription.Quantity,
                SubscriptionName = subscription.SubscriptionName,
                State = newState
            };
        }
    }
}