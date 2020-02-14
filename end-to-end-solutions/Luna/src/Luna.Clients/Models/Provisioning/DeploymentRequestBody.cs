namespace Luna.Clients.Models.Provisioning
{
    public class DeploymentRequestBody
    {
        public string Location { get; set; }
        public DeploymentProperties Properties {get; set; }
    }

    public class ResourceGroupRequestBody
    {
        public string Location { get; set; }
        public string ManagedBy { get; set; }
        public ResourceGroupProperties Properties { get; set; }
        public object Tags { get; set; }
    }
}