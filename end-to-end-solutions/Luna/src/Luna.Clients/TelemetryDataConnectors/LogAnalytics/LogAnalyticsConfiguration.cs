// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Luna.Clients.TelemetryDataConnectors
{
    public class LogAnalyticsConfiguration
    {
        public string WorkspaceId { get; set; }
        public string AADClientId { get; set; }
        public string TenantId { get; set; }
        public string KeyVaultName { get; set; }
        public string AADSecretName { get; set; }
    }
}
