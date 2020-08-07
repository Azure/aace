// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IAPISubscriptionAPIM
    {
        string GETAPIMRESTAPIPath(Guid subscriptionId);

        string GetBaseUrl(string productName, string deploymentName);
        Task<Models.Azure.APISubscription> CreateAsync(APISubscription subscription);
        Task<Models.Azure.APISubscription> UpdateAsync(APISubscription subscription);
        Task DeleteAsync(Data.Entities.APISubscription subscription);
        Task<Models.Azure.APISubscription.Properties> RegenerateKey(Guid subscriptionId, string keyName);
    }
}
