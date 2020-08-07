// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IOperationAPIM
    {
        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName, string operationName);
        public Models.Azure.Operation GetOperation(Models.Azure.OperationTypeEnum operationType);
        public Task<bool> ExistsAsync(APIVersion version, Models.Azure.Operation operation);
        public Task CreateAsync(APIVersion version, Models.Azure.Operation operation);
        public Task UpdateAsync(APIVersion version, Models.Azure.Operation operation);
        public Task DeleteAsync(APIVersion version, Models.Azure.Operation operation);
    }
}
