// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Exceptions
{
    public class LunaConflictUserException : LunaUserException
    {
        public LunaConflictUserException(string message):
            base(message, UserErrorCode.Conflict, System.Net.HttpStatusCode.Conflict)
        {

        }
    }
}
