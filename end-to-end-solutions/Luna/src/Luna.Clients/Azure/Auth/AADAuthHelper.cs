// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Luna.Clients.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob.Protocol;

namespace Luna.Clients.Azure.Auth
{

    public class AADAuthHelper
    {

        public static string[] AdminList;

        public static string AdminTenantId = "common";

        /// <summary>
        /// When applied to a <see cref="HttpContext"/>, verifies that the user authenticated in the 
        /// web API has any of the accepted scopes.
        /// If the authenticated user doesn't have any of these <paramref name="acceptedScopes"/>, the
        /// method throws an HTTP Unauthorized error with a message noting which scopes are expected in the token.
        /// </summary>
        /// <param name="acceptedScopes">Scopes accepted by this API</param>
        /// <exception cref="HttpRequestException"/> with a <see cref="HttpResponse.StatusCode"/> set to 
        /// <see cref="HttpStatusCode.Unauthorized"/>
        public static void VerifyUserHasAnyAcceptedScope(HttpContext context,
                                                         params string[] acceptedScopes)
        {
            if (acceptedScopes == null)
            {
                throw new ArgumentNullException(nameof(acceptedScopes));
            }
            Claim scopeClaim = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/scope");

            if (scopeClaim == null || !scopeClaim.Value.Split(' ').Intersect(acceptedScopes).Any())
            {
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                string message = $"The 'scope' claim does not contain scopes '{string.Join(",", acceptedScopes)}' or was not found";
                throw new HttpRequestException(message);
            }
        }

        public static void VerifyUserAccess(HttpContext context, ILogger logger, bool adminOnly, string subscriptionOwner = "")
        {
            // Verify the access by checking the subscription owner if it is specified. If not, verify if it is admin
            bool hasAccess = false;
            if (!adminOnly && !string.IsNullOrEmpty(subscriptionOwner))
            {
                logger.LogInformation($"Verify user access with subscription owner {subscriptionOwner}.");
                hasAccess = VerifyUserOwnsSubscription(context, subscriptionOwner, logger);
            }
            
            if (!hasAccess)
            {
                logger.LogInformation($"Verify admin access.");
                hasAccess = VerifyAdminTenantAndAccounts(context, logger);
            }

            if (hasAccess)
            {
                logger.LogInformation($"Access verified.");
            }
            else
            {
                throw new LunaUnauthorizedUserException("The resource doesn't exist or you don't have permission to access it.");
            }
        }
        
        public static bool VerifyAdminTenantAndAccounts(HttpContext context, ILogger logger)
        {
            if (AdminTenantId.Equals("common") && AdminList.Length == 0)
            {
                logger.LogInformation("Neither tenant id nor admin accounts are specified. Can't authenticate the ISV portal.");
                return false;
            }

            if (!AdminTenantId.Equals("common", StringComparison.InvariantCultureIgnoreCase))
            {
                string tenantId = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/tenantid").Value;
                if (!tenantId.Equals(AdminTenantId, StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.LogInformation($"The user tenant id {tenantId} doesn't match the required admin tenant id {AdminTenantId}");
                    return false;
                }
            }

            if (AdminList.Length > 0)
            {
                string userAccount = context.User.Identity.Name;
                if (!AdminList.Contains(userAccount))
                {
                    logger.LogInformation($"The user account {userAccount} is not in the admin account list.");
                    return false;
                }
            }

            return true;
        }

        public static bool VerifyUserOwnsSubscription(HttpContext context, string subscriptionOwner, ILogger logger)
        {
            string userAccount = context.User.Identity.Name;
            if (!userAccount.Equals(subscriptionOwner, StringComparison.InvariantCultureIgnoreCase))
            {
                logger.LogInformation($"The user account {userAccount} doesn't match the subscription owner account {subscriptionOwner}.");
                return false;
            }

            return true;
        }
    }
}
