namespace Luna.Data.Enums
{
    public enum ProvisioningState
    {
        ProvisioningPending,
        DeployResourceGroupRunning,
        ArmTemplatePending,
        ArmTemplateRunning,
        WebhookPending,
        NotificationPending,
        // Final states
        DeployResourceGroupFailed,
        ArmTemplateFailed,
        WebhookFailed,
        NotificationFailed,
        ManualActivationPending,
        ManualCompleteOperationPending,
        Succeeded,
        NotSpecified
    }
}
