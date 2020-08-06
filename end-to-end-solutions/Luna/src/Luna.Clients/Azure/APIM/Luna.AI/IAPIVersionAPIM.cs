// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IAPIVersionAPIM
    {
        public string GetAPIMPath(string productName, string deploymentName);
        public string GetControllerBaseUrl();
        public string GetControllerPath(string productName, string deploymentName);
        public string GetOriginAPIMRESTAPIPath(string productName, string deploymentName);
        public string GetAPIMRESTAPIPath(string productName, string deploymentName, string versionName);
        Task<bool> ExistsAsync(APIVersion version);
        Task CreateAsync(APIVersion version);
        Task UpdateAsync(APIVersion version);
        Task DeleteAsync(APIVersion version);
        Task CreateAsync(Deployment deployment);
        Task UpdateAsync(Deployment deployment);
        Task DeleteAsync(Deployment deployment);
       
    }
}
