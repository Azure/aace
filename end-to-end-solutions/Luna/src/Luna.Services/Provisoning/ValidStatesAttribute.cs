// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections.Generic;
using Luna.Data.Enums;

namespace Luna.Services.Provisoning
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ValidStatesAttribute:Attribute
    {
        private List<string> inputStates;

        public ValidStatesAttribute(params ProvisioningState[] states)
        {
            inputStates = new List<string>();
            foreach(var state in states)
            {
                inputStates.Add(state.ToString());
            }
        }

        public List<string> InputStates { get { return inputStates; } }
    }
}
