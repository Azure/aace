// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Luna.Services.Data;

namespace Luna.Services.Utilities.ExpressionEvaluation
{
    /// <summary>
    /// The parameter evaluation context
    /// </summary>
    public class Context
    {
        private static Random random = new Random();
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static bool isTestContext = false;

        public Dictionary<string, object> Parameters = new Dictionary<string, object>();

        public static IIpAddressService _ipAddressService;

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionOwner">The subscription owner</param>
        /// <param name="subscriptionId">The subscription Id</param>
        /// <param name="planName">The plan name</param>
        /// <param name="operationType">The operation type</param>
        public Context(string offerName, string subscriptionOwner, Guid subscriptionId, string planName, string operationType)
        {
            // If you add any parameter here, also add it to the reserved parameter name in ExpressionEvaluationUtils.cs so ISV won't use it.
            Parameters.Add(ExpressionEvaluationUtils.OfferNameParameterName, offerName);
            Parameters.Add(ExpressionEvaluationUtils.SubscriptionOwnerParameterName, subscriptionOwner);
            Parameters.Add(ExpressionEvaluationUtils.SubscriptionIdParameterName, subscriptionId);
            Parameters.Add(ExpressionEvaluationUtils.PlanNameParameterName, planName);
            Parameters.Add(ExpressionEvaluationUtils.OperationTypeParameterName, operationType);
        }

        /// <summary>
        /// Get a random string with specified length
        /// </summary>
        /// <param name="length">The length of the random string</param>
        /// <returns></returns>
        public static string GetRandomString(int length)
        {
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Get a new ip range for current subscription.
        /// The ip range will be marked as used
        /// </summary>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionid">the subscription id</param>
        /// <param name="ipConfigName">The ip config name</param>
        /// <returns></returns>
        public static string GetIpRange(object offerName, object subscriptionid, string ipConfigName)
        {
            return _ipAddressService.TryAssignIpAddress(new Guid(subscriptionid.ToString()), offerName.ToString(), ipConfigName).Result.Value;
        }

        /// <summary>
        /// Check if a number is power of 2
        /// </summary>
        /// <param name="n">The number</param>
        /// <returns>Of the number is power of 2</returns>
        private static bool isPowerOfTwo(int n)
        {
            return (int)(Math.Ceiling((Math.Log(n) / Math.Log(2))))
                  == (int)(Math.Floor(((Math.Log(n) / Math.Log(2)))));
        }

        /// <summary>
        /// Get sub ip range
        /// </summary>
        /// <param name="ipRangeObj">The ip range.</param>
        /// <param name="start">The start ip</param>
        /// <param name="length">The length of the sub range</param>
        /// <returns></returns>
        public static string GetSubIpRange(object ipRangeObj, int start, int length)
        {
            string ipRange = ipRangeObj.ToString();
            if (!isPowerOfTwo(length))
            {
                throw new ArgumentException("The length of subrange must by power of 2.");
            }
            string startIp = ipRange.Substring(0, ipRange.IndexOf("/"));
            int ipRangeSize = PrefixToLength(Int32.Parse(ipRange.Substring(ipRange.IndexOf("/") + 1, ipRange.Length - ipRange.IndexOf("/") - 1)));
            if (ipRangeSize < start + length)
            {
                throw new ArgumentOutOfRangeException(string.Format("The specified ipRange has {0} ip addresses. The sub-range {1} to {2} is out of range.",
                    ipRangeSize, start, start + length));
            }

            string startIpWithOffset = CalculateIpAddressWithOffset(startIp, start);
            int subRangePrefix = LengthToPrefix(length);
            return string.Format("{0}/{1}", startIpWithOffset, subRangePrefix);
        }

        /// <summary>
        /// Convert prefix to lengh of ip range
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <returns>The length of ip range</returns>
        private static int PrefixToLength(int prefix)
        {
            if (prefix < 0 || prefix > 32)
            {
                throw new ArgumentException("The prefix must be an integer between 0 and 32.");
            }
            int result = 1;
            for (int i = 32; i > prefix; i--)
            {
                result = result * 2;
            }
            return result;
        }

        /// <summary>
        /// Convert length of ip range to prefix
        /// </summary>
        /// <param name="length">The length of ip range</param>
        /// <returns>The prefix</returns>
        private static int LengthToPrefix(int length)
        {
            if (length < 1)
            {
                throw new ArgumentException("The length of the ip range much be greater than 1.");
            }
            if (!isPowerOfTwo(length))
            {
                throw new ArgumentException("The length of the ip range must by power of 2.");
            }

            for (int i = 32; i > 0; i--)
            {
                if (length <= 1)
                {
                    return i;
                }
                length = length / 2;
            }

            return 0;

        }

        /// <summary>
        /// Calculate ip addresses with offset
        /// </summary>
        /// <param name="startIp">The start ip address</param>
        /// <param name="offset">The offset</param>
        /// <returns>The new ip address with offset</returns>
        private static string CalculateIpAddressWithOffset(string startIp, int offset)
        {
            IPAddress ip = IPAddress.Parse(startIp);
            var addressBytes = ip.GetAddressBytes();
            for (int i = 3; offset > 0 && i >= 0; i--)
            {
                var address = addressBytes[i];
                addressBytes[i] = (byte)((address + offset) % 256);
                offset = (address + offset) / 256;
            }
            return new IPAddress(addressBytes).ToString();
        }
    }
}
