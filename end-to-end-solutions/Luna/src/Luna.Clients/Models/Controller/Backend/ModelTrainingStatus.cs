// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.Controller.Backend
{
    public class ModelTrainingStatus
    {
        public string runUuid { get; set; }
        public string rootRunUuid { get; set; }
        public string status { get; set; }
        public string startTimeUtc { get; set; }
        public string endTimeUtc { get; set; }
        public object error { get; set; }
    }
}
