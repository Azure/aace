using System;
using System.Collections.Generic;
using System.Text;

namespace Luna.Clients.Models.CustomMetering
{
    public enum CustomMeterEventStatus
    {
        Accepted,
        Expired,
        Duplicate,
        Error,
        ResourceNotFound,
        ResourceNotAuthorized,
        InvalidDimension,
        InvalidQuantity,
        BadArgument
    }
}
