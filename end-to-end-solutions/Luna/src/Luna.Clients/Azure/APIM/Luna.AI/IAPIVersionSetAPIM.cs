// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IAPIVersionSetAPIM
    {
        string GetAPIMRESTAPIPath(string productName, string deploymentName);
        Task CreateAsync(Deployment deployment);
        Task UpdateAsync(Deployment deployment);
        Task DeleteAsync(Deployment deployment);
        
    }
}
