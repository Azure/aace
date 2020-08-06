// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;

namespace Luna.Clients.Exceptions
{
    public class LunaFulfillmentException : LunaServerException
    {
        public LunaFulfillmentException(
            string message,
            LunaServerException innerException = default) : base(
                message,
                innerException.IsRetryable,
                innerException) { }

        public LunaFulfillmentException(
            string message,
            bool isRetryable,
            Exception innerException = default) : base(
                message,
                isRetryable,
                innerException) { }

        public LunaFulfillmentException(
            string message,
            Exception innerException = default) : base(
                message,
                false,
                innerException) { }
    }
}
