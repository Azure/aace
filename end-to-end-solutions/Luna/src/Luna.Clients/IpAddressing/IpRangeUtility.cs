// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;

/*
==============================================
Based on the Stack Overflow post linked below.
https://stackoverflow.com/a/4172982
==============================================
*/

namespace Luna.Clients.IpAddressing
{
    public static class IpRangeUtility
    {
        /// <summary>
        /// Gets all IP addresses in the range of the provided IPv4 address in CIDR notation. 
        /// </summary>
        /// <param name="ipRange">The IPv4 address to enumerate.</param>
        /// <returns>A list of IPv4 addresses as strings.</returns>
        public static List<string> GetAllIpsInRange(string ipRange)
        {
            Tuple<byte[], byte[]> range = SetStartAndEndIp(ipRange);
            byte[] startIp = range.Item1;
            byte[] endIp = range.Item2;

            int capacity = 1;
            for (int i = 0; i < 4; i++)
                capacity *= endIp[i] - startIp[i] + 1;

            List<string> ips = new List<string>(capacity);
            for (int i0 = startIp[0]; i0 <= endIp[0]; i0++)
            {
                for (int i1 = startIp[1]; i1 <= endIp[1]; i1++)
                {
                    for (int i2 = startIp[2]; i2 <= endIp[2]; i2++)
                    {
                        for (int i3 = startIp[3]; i3 <= endIp[3]; i3++)
                        {
                            ips.Add(IpAddressToString(new byte[] { (byte)i0, (byte)i1, (byte)i2, (byte)i3 }));
                        }
                    }
                }
            }

            return  ips;
        }

        /// <summary>
        /// Sets the startIp and endIp variables.
        /// </summary>
        /// <param name="ipRange">The IPv4 CIDR range to operate on.</param>
        private static Tuple<byte[], byte[]> SetStartAndEndIp(string ipRange)
        {
            byte[] startIp;
            byte[] endIp;

            string[] x = ipRange.Split('/');

            byte bits = byte.Parse(x[1]);
            uint ip = 0;
            String[] ipParts0 = x[0].Split('.');
            for (int i = 0; i < 4; i++)
            {
                ip = ip << 8;
                ip += uint.Parse(ipParts0[i]);
            }

            byte shiftBits = (byte)(32 - bits);
            uint ip1 = (ip >> shiftBits) << shiftBits;

            uint ip2 = ip1 >> shiftBits;
            for (int k = 0; k < shiftBits; k++)
            {
                ip2 = (ip2 << 1) + 1;
            }

            startIp = new byte[4];
            endIp = new byte[4];

            for (int i = 0; i < 4; i++)
            {
                startIp[i] = (byte) ((ip1 >> (3 - i) * 8) & 255);
                endIp[i] = (byte)((ip2 >> (3 - i) * 8) & 255);
            }

            return new Tuple<byte[], byte[]>(startIp, endIp);
        }

        /// <summary>
        /// ToString method that converts a byte array into an IPv4 address.
        /// </summary>
        /// <param name="sections">Byte array with exactly 4 elements where each element represents the value in each section of an IPv4 address.</param>
        /// <returns></returns>
        private static string IpAddressToString(byte[] sections)
        {
            // Exactly four sections must be provided that compose the IP address
            if (sections is null || sections.Count() != 4)
            {
                // TODO
                throw new NotSupportedException();
            }

            return  sections.ElementAt(0).ToString() + "." +
                    sections.ElementAt(1).ToString() + "." +
                    sections.ElementAt(2).ToString() + "." +
                    sections.ElementAt(3).ToString();
        }
    }
}