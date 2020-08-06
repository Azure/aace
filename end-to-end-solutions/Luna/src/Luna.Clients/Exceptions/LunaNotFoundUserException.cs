// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿namespace Luna.Clients.Exceptions
{
    public class LunaNotFoundUserException:LunaUserException
    {
        public LunaNotFoundUserException(string message):
            base(message, UserErrorCode.ResourceNotFound, System.Net.HttpStatusCode.NotFound)
        {

        }
    }
}
