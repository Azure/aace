// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;

namespace Luna.Clients.Exceptions
{
    public class LunaDbSaveException : LunaServerException
    {
        public LunaDbSaveException(
            string message,
            LunaServerException innerException = default) : base(
                message,
                innerException.IsRetryable,
                innerException)
        {

        }

        public LunaDbSaveException(
            string message,
            bool isRetryable,
            Exception innerException = default) : base(
                message,
                isRetryable,
                innerException)
        {

        }

        public LunaDbSaveException(
            string message,
            Exception innerException = default) : base(
                message,
                false,
                innerException)
        { 
        
        }
    }
}
