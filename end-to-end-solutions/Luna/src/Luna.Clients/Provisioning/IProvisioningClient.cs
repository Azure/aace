using System;
using System.Threading;
using System.Threading.Tasks;
using Luna.Clients.Models.Provisioning;

namespace Luna.Clients.Provisioning
{
    public interface IProvisioningClient 
    {
        Task<DeploymentExtendedResult> PutDeploymentAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId, 
            string resourceGroup, 
            string deploymentName, 
            string templatePath = default,
            object template = default,
            string parametersPath = default,
            object parameters = default,
            bool rollbackToLastSuccessful = default,
            CancellationToken cancellationToken = default
        );

        Task<DeploymentExtendedResult> GetDeploymentAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId, 
            string resourceGroup, 
            string deploymentName,
            CancellationToken cancellationToken = default
        );

        Task<DeploymentValidateResult> ValidateTemplateAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            string deploymentName,
            string deploymentMode,
            string templatePath,
            string parameterPath,
            CancellationToken cancellationToken = default
        );

        Task<ResourceGroupResult> CreateOrUpdateResourceGroupAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            string location,
            CancellationToken cancellationToken = default
        );

        Task<bool> ResourceGroupExistsAsync(
            Guid requestId,
            Guid correlationId,
            string subscriptionId,
            string resourceGroup,
            CancellationToken cancellationToken = default
        );

        Task<bool> ExecuteWebhook(Uri uri);
    }
}
