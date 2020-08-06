// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Threading.Tasks;
using Luna.Data.Entities;

namespace Luna.Clients.Azure.APIM
{
    public interface IUserAPIM
    {
        string GetUserName(string owner);
        string GetAPIMRESTAPIPath(string owner);
        Task CreateAsync(string owner);
        Task UpdateAsync(string owner);
        Task DeleteAsync(string owner);
    }
}
