// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.ComponentModel;

namespace Luna.Clients.Fulfillment
{
    public enum SubscriptionAction
    {
        Activate,
        Subscribe,
        Update,
        Unsubscribe,
        Suspend,
        DeleteData,
        Reinstate,
    }

    public static class SubscriptionActionExtensions
    {
        public static string ToVerb(this SubscriptionAction action)
        {
            switch (action)
            {
                case SubscriptionAction.Activate:
                    return "activate";

                case SubscriptionAction.Subscribe:
                    return "subscribe";

                case SubscriptionAction.Update:
                    return "update";

                case SubscriptionAction.Unsubscribe:
                    return "unsubscribe from";

                case SubscriptionAction.Suspend:
                    return "suspend";

                case SubscriptionAction.DeleteData:
                    return "delete data for";

                case SubscriptionAction.Reinstate:
                    return "reinstate";

                default:
                    throw new InvalidEnumArgumentException();
            }
        }
    }
}