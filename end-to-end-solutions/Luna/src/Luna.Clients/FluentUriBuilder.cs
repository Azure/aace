// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Linq;
using System.Web;

namespace Luna.Clients
{
    public class FluentUriBuilder
    {
        private readonly UriBuilder uriBuilder;

        public FluentUriBuilder(UriBuilder uriBuilder)
        {
            this.uriBuilder = uriBuilder;
        }

        public Uri Uri => this.uriBuilder.Uri;

        public static FluentUriBuilder Start(string uriString)
        {
            return new FluentUriBuilder(new UriBuilder(uriString.TrimEnd('/')));
        }

        public FluentUriBuilder AddPath(string path)
        {
            _ = path.Trim('/');
            this.uriBuilder.Path += this.uriBuilder.Path == "/" ? $"{path}" : $"/{path}";

            return this;
        }

        public FluentUriBuilder AddQuery(string queryParameterName, string queryParameter)
        {
            var charsToRemove = new[] { '&', '?', '=' };

            var cleanParameterName = queryParameterName.Trim(charsToRemove);
            var cleanParameter = queryParameter.Trim(charsToRemove);

            var currentQuery = HttpUtility.ParseQueryString(this.uriBuilder.Uri.Query);
            currentQuery.Add(queryParameterName, queryParameter);
            this.uriBuilder.Query = currentQuery.AllKeys.Select(k => $"{k}={currentQuery[k]}")
                .Aggregate((working, next) => $"{working}&{next}");

            return this;
        }
    }
}
