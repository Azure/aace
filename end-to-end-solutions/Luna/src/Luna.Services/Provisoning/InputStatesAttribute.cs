using System;
using Luna.Data.Enums;

namespace Luna.Services.Provisoning
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InputStatesAttribute: ValidStatesAttribute
    {
        public InputStatesAttribute(params ProvisioningState[] states):base(states)
        {

        }
    }
}
