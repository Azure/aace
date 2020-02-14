using System;

namespace Luna.Clients.Azure.Storage
{
    public class StorageAccountConfigurationOption
    {
        public StorageAccountConfiguration Config { get; set; }
    }

    public class StorageAccountConfiguration
    {
        public string VaultName { get; set; }
        public string AccountName { get; set; }
        public string AccountKey { get; set; }
    }
}
