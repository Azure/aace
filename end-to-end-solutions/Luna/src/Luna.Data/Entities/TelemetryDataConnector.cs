// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Luna.Data.Entities
{
    public partial class TelemetryDataConnector
    {
        public TelemetryDataConnector()
        {

        }

        public void Copy(TelemetryDataConnector telemetryDataConnector)
        {
            this.Name = telemetryDataConnector.Name;
            this.Type = telemetryDataConnector.Type;
            this.Configuration = telemetryDataConnector.Configuration;
        }

        [JsonIgnore]
        public long Id { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

        public string Configuration { get; set; }

        [JsonIgnore]
        public virtual ICollection<CustomMeter> CustomMeters { get; set; }
    }
}
