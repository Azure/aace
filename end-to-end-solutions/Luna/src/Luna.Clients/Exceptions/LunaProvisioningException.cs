// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using Luna.Data.Enums;

namespace Luna.Clients.Exceptions
{
    public class LunaProvisioningException : LunaServerException
    {
        public LunaProvisioningException(
            string message,
            bool isRetryable = default,
            ProvisioningState failbackState = ProvisioningState.NotSpecified,
            Exception innerException = default) :
            base(message, isRetryable, innerException)
        {
            FailbackState = failbackState;
        }

        public ProvisioningState FailbackState { get; set; }
    }
}
