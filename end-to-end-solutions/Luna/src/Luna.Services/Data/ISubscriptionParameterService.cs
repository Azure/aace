// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Services.Data
{
    /// <summary>
    /// Interface that handles basic CRUD functionality for the subscriptionParameter resource.
    /// </summary>
    public interface ISubscriptionParameterService
    {
        Task<List<SubscriptionParameter>> GetAllAsync(Guid subscriptionId);

        Task<SubscriptionParameter> GetAsync(Guid subscriptionId, string name);

        Task<SubscriptionParameter> CreateAsync(SubscriptionParameter armTemplateParameter);
        
        Task<bool> ExistsAsync(Guid subscriptionId, string name);
    }
}