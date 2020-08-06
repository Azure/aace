// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Linq;
using System.Net.Http.Headers;

namespace Luna.Clients.Models.Fulfillment
{
    public class UpdateOrDeleteSubscriptionRequestResult : FulfillmentRequestResult
    {
        private const string OperationLocationKey = "Operation-Location";
        private const string RetryAfterKey = "Retry-After";

        public Guid OperationId { get; set; }

        public int RetryAfter { get; set; }

        protected override void UpdateFromHeaders(HttpHeaders headers)
        {
            base.UpdateFromHeaders(headers);

            if (headers.TryGetValues(OperationLocationKey, out var values))
            {
                Uri.TryCreate(values.First(), UriKind.Absolute, out var operationUri);
                if (operationUri == default)
                {
                    throw new ApplicationException("API did not return an operation ID");
                }

                // The URI should be like https://marketplaceapi.microsoft.com/api/saas/subscriptions/1be86829-c7ec-1738-ab03-a6cacebe3832/operations/ed10f0b7-6cd6-416d-b015-83c11c9f083b?api-version=2018-08-31
                // So segments should look like
                /*
                    /
                    api/
                    saas/
                    subscriptions/
                    1be86829-c7ec-1738-ab03-a6cacebe3832/
                    operations/
                    ed10f0b7-6cd6-416d-b015-83c11c9f083b
                */

                if (operationUri.Segments.Length != 7)
                {
                    throw new ApplicationException("URI is not recognized as an operation ID url");
                }

                if (!Guid.TryParse(operationUri.Segments[6], out var operationGuid))
                {
                    throw new ApplicationException("Returned operation ID is not a Guid");
                }

                this.OperationId = operationGuid;
            }

            if (!headers.TryGetValues(RetryAfterKey, out values))
            {
                return;
            }

            int.TryParse(values.First(), out var retryAfter);

            if (retryAfter == 0)
            {
                throw new ApplicationException("API did not return a retry-after value");
            }

            this.RetryAfter = retryAfter;
        }
    }
}
