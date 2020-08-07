// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Luna.Clients.TelemetryDataConnectors
{
    public class QueryResponse
    {
        public List<ResultTable> tables { get; set; }
    }

    public class ResultTable
    {
        public string name { get; set; }
        public List<ResultColumn> columns { get; set; }
        public List<List<object>> rows { get; set; }
    }

    public class ResultColumn
    {
        public string name { get; set; }
        public string type { get; set; }
    }


}
