// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IProductAPIVersionAPIM
    {
        string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName);
        Task<bool> ExistsAsync(APIVersion version);
        Task CreateAsync(APIVersion version);
        Task UpdateAsync(APIVersion version);
        Task DeleteAsync(APIVersion version);
    }
}
