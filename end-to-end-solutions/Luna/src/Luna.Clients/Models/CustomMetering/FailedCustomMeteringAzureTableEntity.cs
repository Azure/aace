using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.KeyVault.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace Luna.Clients.Models.CustomMetering
{
    public class FailedCustomMeteringAzureTableEntity : CustomMeteringAzureTableEntity
    {
        public FailedCustomMeteringAzureTableEntity(CustomMeteringSuccessResult result, string errorMessage): base(result)
        {
            ErrorMessage = errorMessage;
        }

        public string ErrorMessage { get; set; }

    }
}
