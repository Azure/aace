using System;
using System.Threading;
using System.Threading.Tasks;
using Luna.Data.Entities;
using Luna.Services.Utilities;

namespace Luna.Services
{
    /// <summary>
    /// Map marketplace notifications to provisioning actions.
    /// </summary>
    public interface IMarketplaceNotificationHandler
    {
        Task ProcessStartProvisioningAsync(
            Subscription subscription,
            string azureLocation, 
            CancellationToken cancellationToken = default
        );
            
        Task ProcessChangePlanAsync(
            Subscription subscription,
            Guid operationId,
            CancellationToken cancellationToken = default
        );
        
        Task ProcessChangeQuantityAsync(
            Subscription subscription,
            CancellationToken cancellationToken = default
        );

        Task ProcessUnsubscribeAsync(
            Subscription subscription,
            Guid operationId,
            CancellationToken cancellationToken = default
        );
            
        Task ProcessChangePlanAsync(
            NotificationModel notificationModel, 
            CancellationToken cancellationToken = default
        );

        Task ProcessChangeQuantityAsync(
            NotificationModel notificationModel, 
            CancellationToken cancellationToken = default
        );

        Task ProcessOperationFailOrConflictAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default
        );

        Task ProcessReinstatedAsync(
            NotificationModel notificationModel, 
            CancellationToken cancellationToken = default
        );

        Task ProcessSuspendedAsync(
            NotificationModel notificationModel, 
            CancellationToken cancellationToken = default
        );

        Task ProcessUnsubscribeAsync(
            NotificationModel notificationModel,
            CancellationToken cancellationToken = default
        );
    }
}