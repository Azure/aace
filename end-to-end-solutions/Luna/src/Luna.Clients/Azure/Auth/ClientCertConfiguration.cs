// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Azure.Auth
{
    public class ClientCertConfiguration
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public CertProp Properties { get; set; }
    }

    public class CertProp
    {
        public string Subject { get; set; }
        public string Thumbprint { get; set; }
        public string ExpirationDate { get; set; }
    }
}
