// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace Luna.Clients.Models.Provisioning
{
    /// <summary>
    /// Deployment information
    /// </summary>
    public class DeploymentExtendedResult : DeploymentRequestResult
    {
        /// <summary>
        /// The ID of the deployment
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// The location of the deployment
        /// </summary>
        /// <value></value>
        public string Location { get; set; }
        /// <summary>
        /// The name of the deployment
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// Deployment properties
        /// </summary>
        /// <value></value>
        public DeploymentPropertiesExtended Properties { get; set; }
        /// <summary>
        /// The type of the deployment
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
    }

    /// <summary>
    /// Deployment properties
    /// </summary>
    public class DeploymentProperties
    {
        /// <summary>
        /// The debug setting of the deployment
        /// </summary>
        /// <value></value>
        public DebugSetting DebugSetting { get; set; }
        /// <summary>
        /// The deployment mode. Possible values are Incremental and Complete
        /// </summary>
        /// <value></value>
        public string Mode { get; set; }
        /// <summary>
        /// The deployment on error behavior
        /// </summary>
        /// <value></value>
        public OnErrorDeployment OnErrorDeployment { get; set; }
        /// <summary>
        /// Deployment parameters
        /// Use only one of Parameters or ParametersLink
        /// </summary>
        /// <value></value>
        public object Parameters { get; set; }
        /// <summary>
        /// The URI referencing the parameters
        /// Use only one of Parameters or ParametersLink
        /// </summary>
        /// <value></value>
        public ParametersLink ParametersLink { get; set; }
        /// <summary>
        /// The template content
        /// Use only one of Template or TemplateLink
        /// </summary>
        /// <value></value>
        public object Template { get; set; }
        /// <summary>
        /// The URI referencing the template
        /// Use only one of Template or TemplateLink
        /// </summary>
        /// <value></value>
        public TemplateLink TemplateLink { get; set; }
    }

    /// <summary>
    /// Deployment Properties with additional details
    /// </summary>
    public class DeploymentPropertiesExtended
    {
        /// <summary>
        /// The correlation ID of the deployment
        /// </summary>
        /// <value></value>
        public string CorrelationId { get; set; }
        /// <summary>
        /// The debug setting of the deployment
        /// </summary>
        /// <value></value>
        public DebugSetting DebugSetting { get; set; }
        /// <summary>
        /// The list of deployment dependencies
        /// </summary>
        /// <value></value>
        public Dependency[] Dependencies { get; set; }
        /// <summary>
        /// The duration of the template deployment
        /// </summary>
        /// <value></value>
        public string Duration { get; set; }
        /// <summary>
        /// The deployment mode. Possible values are Incremental and Complete
        /// </summary>
        /// <value></value>
        public string Mode { get; set; }
        /// <summary>
        /// The deployment on error behavior
        /// </summary>
        /// <value></value>
        public OnErrorDeployment OnErrorDeployment { get; set; }
        /// <summary>
        /// Key/value pairs that represent deployment output
        /// </summary>
        /// <value></value>
        public object Outputs { get; set; }
        /// <summary>
        /// Deployment parameters
        /// Use only one of Parameters or ParametersLink
        /// </summary>
        /// <value></value>
        public object Parameters { get; set; }
        /// <summary>
        /// The URI referencing the parameters
        /// Use only one of Parameters or ParametersLink
        /// </summary>
        /// <value></value>
        public ParametersLink ParametersLink { get; set; }
        /// <summary>
        /// The list of resource providers needed for the deployment
        /// </summary>
        /// <value></value>
        public Provider[] Providers { get; set; }
        /// <summary>
        /// The state of the provisioning
        /// </summary>
        /// <value></value>
        public string ProvisioningState { get; set; }
        /// <summary>
        /// The template content
        /// Use only one of Template or TemplateLink
        /// </summary>
        /// <value></value>
        public object Template { get; set; }
        /// <summary>
        /// The URI referencing the template
        /// Use only one of Template or TemplateLink
        /// </summary>
        /// <value></value>
        public TemplateLink TemplateLink { get; set; }
        /// <summary>
        /// The timestamp of the template deployment
        /// </summary>
        /// <value></value>
        public string Timestamp { get; set; }
    }

    /// <summary>
    /// The debug setting
    /// </summary>
    public class DebugSetting
    {
        /// <summary>
        /// Specifies the type of information to log for debugging
        /// The permitted values are none, requestContent, responseContent, or both requestContent and responseContent separated by a comma
        /// The default is none
        /// </summary>
        /// <value></value>
        public string DetailLevel { get; set; }
    }

    /// <summary>
    /// Deployment dependency information
    /// </summary>
    public class Dependency
    {
        /// <summary>
        /// The list of dependencies
        /// </summary>
        /// <value></value>
        public BasicDependency[] DependsOn { get; set; }
        /// <summary>
        /// The ID of the dependency
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// The dependency resource name
        /// </summary>
        /// <value></value>
        public string ResourceName { get; set; }
        /// <summary>
        /// The dependency resource type
        /// </summary>
        /// <value></value>
        public string ResourceType { get; set; }
    }

    /// <summary>
    /// Deployment dependency information
    /// </summary>
    public class BasicDependency
    {
        /// <summary>
        /// The ID of the dependency
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// The dependency resource name
        /// </summary>
        /// <value></value>
        public string ResourceName { get; set; }
        /// <summary>
        /// The dependency resource type
        /// </summary>
        /// <value></value>
        public string ResourceType { get; set; }
    }

    /// <summary>
    /// Deployment on error behavior with additional details
    /// </summary>
    public class OnErrorDeployment
    {
        /// <summary>
        /// The deployment to be used on error case
        /// </summary>
        /// <value></value>
        public string DeploymentName { get; set; }
        /// <summary>
        /// The state of the provisioning for the on error deployment
        /// </summary>
        /// <value></value>
        public string ProvisioningState { get; set; }
        /// <summary>
        /// The deployment on error behavior type. Possible values are LastSuccessful and SpecificDeployment
        /// </summary>
        /// <value></value>
        public string Type { get; set; }    
    }

    /// <summary>
    /// Entity representing the reference to the deployment parameters
    /// </summary>
    public class ParametersLink
    {
        /// <summary>
        /// If included, must match the ContentVersion in the template
        /// </summary>
        /// <value></value>
        public string ContentVersion { get; set; }
        /// <summary>
        /// The URI of the parameters file
        /// </summary>
        /// <value></value>
        public string Uri { get; set; }
    }

    /// <summary>
    /// Resource provider information
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// The provider ID
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// The namespace of the resource provider
        /// </summary>
        /// <value></value>
        public string Namespace { get; set; }
        /// <summary>
        /// The registration policy of the resource provider
        /// </summary>
        /// <value></value>
        public string RegistrationPolicy { get; set; }
        /// <summary>
        /// The registration state of the resource provider
        /// </summary>
        /// <value></value>
        public string RegistrationState { get; set; }
        /// <summary>
        /// The collection of provider resource types
        /// </summary>
        /// <value></value>
        public ProviderResourceType[] ResourceTypes { get; set; }
    }

    /// <summary>
    /// Resource type managed by the resource provider
    /// </summary>
    public class ProviderResourceType
    {
        /// <summary>
        /// The aliases that are supported by this resource type
        /// </summary>
        /// <value></value>
        public AliasType[] Aliases { get; set; }
        /// <summary>
        /// The API version
        /// </summary>
        /// <value></value>
        public string[] ApiVersions { get; set; }
        /// <summary>
        /// The additional capabilities offered by this resource type
        /// </summary>
        /// <value></value>
        public string Capabilities { get; set; }
        /// <summary>
        /// The collection of locations where this resource type can be created
        /// </summary>
        /// <value></value>
        public string[] Locations { get; set; }
        /// <summary>
        /// The properties
        /// </summary>
        /// <value></value>
        public object Properties { get; set; }
        /// <summary>
        /// The resource type
        /// </summary>
        /// <value></value>
        public string ResourceType { get; set; }
    }

    /// <summary>
    /// The alias type
    /// </summary>
    public class AliasType
    {
        /// <summary>
        /// The alias name
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// The paths for an alias
        /// </summary>
        /// <value></value>
        public AliasPathType Paths { get; set; }
    }

    /// <summary>
    /// The type of the paths for alias
    /// </summary>
    public class AliasPathType
    {
        /// <summary>
        /// The API versions
        /// </summary>
        /// <value></value>
        public string[] ApiVersions { get; set; }
        /// <summary>
        /// The path of an alias
        /// </summary>
        /// <value></value>
        public string Path { get; set; }
    }

    /// <summary>
    /// Entity representing the reference to the template
    /// </summary>
    public class TemplateLink
    {
        /// <summary>
        /// If included, must match the ContentVersion in the template
        /// </summary>
        /// <value></value>
        public string ContentVersion { get; set; }
        /// <summary>
        /// The URI of the template to deploy
        /// </summary>
        /// <value></value>
        public string Uri { get; set; }
    }

    /// <summary>
    /// Resource group information
    /// </summary>
    public class ResourceGroupResult : DeploymentRequestResult
    {
        /// <summary>
        /// The id of the resource group
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// The location of the resource group.await This cannot be changed after the rg has been created
        /// </summary>
        /// <value></value>
        public string Location { get; set; }
        /// <summary>
        /// The id of the resource that manages the resource group
        /// </summary>
        /// <value></value>
        public string ManagedBy { get; set; }
        /// <summary>
        /// The name of the resource group
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// The resource group properties
        /// </summary>
        /// <value></value>
        public ResourceGroupProperties Properties { get; set; }
        /// <summary>
        /// The tags attached to the resource group
        /// </summary>
        /// <value></value>
        public object Tags { get; set; }
        /// <summary>
        /// The type of the resource group
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
    }

    /// <summary>
    /// The resource group properties
    /// </summary>
    public class ResourceGroupProperties
    {
        /// <summary>
        /// The provisioning state
        /// </summary>
        /// <value></value>
        public string ProvisioningState { get; set; }
    }

    /// <summary>
    /// Information from validate template deployment response
    /// </summary>
    public class DeploymentValidateResult : DeploymentRequestResult
    {
        /// <summary>
        /// Deployment id
        /// </summary>
        /// <value></value>
        public string Id { get; set; }
        /// <summary>
        /// Deployment name
        /// </summary>
        /// <value></value>
        public string Name { get; set; }
        /// <summary>
        /// Deployment type (e.g. "Microsoft.Resources/deployments")
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
        /// <summary>
        /// The deployment validation error
        /// </summary>
        /// <value></value>
        public ErrorResponse Error { get; set; }
        /// <summary>
        /// The template deployment properties
        /// </summary>
        /// <value></value>
        public DeploymentPropertiesExtended Properties { get; set; }
    }

    /// <summary>
    /// The resource management error response
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// The error additional info
        /// </summary>
        /// <value></value>
        public ErrorAdditionalInfo[] AdditionalInfo { get; set; }
        /// <summary>
        /// The error code
        /// </summary>
        /// <value></value>
        public string Code { get; set; }
        /// <summary>
        /// The error details
        /// </summary>
        /// <value></value>
        public ErrorResponse[] Details { get; set; }
        /// <summary>
        /// The error message
        /// </summary>
        /// <value></value>
        public string Message { get; set; }
        /// <summary>
        /// The error target
        /// </summary>
        /// <value></value>
        public string Target { get; set; }
    }

    /// <summary>
    /// Resource management error additional info
    /// </summary>
    public class ErrorAdditionalInfo
    {
        /// <summary>
        /// The additional info
        /// </summary>
        /// <value></value>
        public object Info { get; set; }
        /// <summary>
        /// The additional info type
        /// </summary>
        /// <value></value>
        public string Type { get; set; }
    }
}