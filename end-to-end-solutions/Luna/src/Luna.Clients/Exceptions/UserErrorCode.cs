// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿namespace Luna.Clients.Exceptions
{
    public enum UserErrorCode
    {
        ResourceNotFound,
        PayloadNotProvided,
        NameMismatch,
        Conflict,
        ParameterNameIsReserved,
        ParameterNotProvided,
        ArmTemplateNotProvided,
        InvalidParameter,
        Unauthorized,
        AuthKeyNotProvided,
        PayloadNameInvalid
    }
}
