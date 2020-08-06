// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IPolicyAPIM
    {
        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName, string operationName);

        public Task<bool> ExistsAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType);

        public Task CreateAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType);

        public Task UpdateAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType);

        public Task DeleteAsync(APIVersion version, string operationName, Models.Azure.OperationTypeEnum operationType);
        
    }
}
