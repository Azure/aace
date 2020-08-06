// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System.Net.Mail;

namespace Luna.Services.Utilities
{
    public static class StringExtensions
    {
        public static string GetDomainNameFromEmail(this string emailString)
        {
            try
            {
                var email = new MailAddress(emailString);
                return email.Host;
            }
            catch
            {
                return "InvalidCustomer";
            }
        }

        public static bool IsValidEmail(this string email)
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}