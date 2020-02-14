namespace Luna.Services.Utilities
{
    public class DashboardOptions
    {
        public string BaseUrl { get; set; }

        public string DashboardAdmin { get; set; }

        public MailOptions Mail { get; set; }
    }

    public class AzureOptions
    {
        public string Location { get; set; }
    }

    public class StorageOptions
    {
        public string StorageAccountName { get; set; }
        public string StorageAccountKey { get; set; }
    }
}