// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Luna.Data.Enums
{
    public enum ArmProvisioningState
    {
        Updating,
        Succeeded,
        Failed,
        Canceled
    }
}
