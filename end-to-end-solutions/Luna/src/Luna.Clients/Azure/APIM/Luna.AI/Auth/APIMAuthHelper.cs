// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Luna.Clients.Azure.APIM
{
    public class APIMAuthHelper
    {
        private string _id;
        private string _key;
        private DateTime expireTime;
        public APIMAuthHelper(string id, string key)
        {
            _id = id;
            _key = key;
            expireTime = DateTime.Now;
        }
        public string GetSharedAccessToken()
        {
            var key = string.Format("{0}", _key);
            if (expireTime.Subtract(DateTime.Now).TotalDays < 1) expireTime = DateTime.UtcNow.AddDays(30);
            var expiry = expireTime;
            using (var encoder = new HMACSHA512(Encoding.UTF8.GetBytes(key)))
            {
                var dataToSign = _id + "\n" + expiry.ToString("O", CultureInfo.InvariantCulture);
                var hash = encoder.ComputeHash(Encoding.UTF8.GetBytes(dataToSign));
                var signature = Convert.ToBase64String(hash);
                var encodedToken = string.Format("uid={0}&ex={1:o}&sn={2}", _id, expiry, signature);
                return encodedToken;
            }
        }
    }
}
