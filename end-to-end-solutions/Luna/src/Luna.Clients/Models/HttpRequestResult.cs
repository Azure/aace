// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Luna.Clients.Models
{
    public class HttpRequestResult
    {
        private const string RequestIdKey = "x-ms-requestid";

        public HttpRequestResult()
        {
            this.Success = false;
        }

        public string RawResponse { get; internal set; }

        public Guid RequestId { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public bool Success { get; set; }

        public static async Task<T> ParseAsync<T>(HttpResponseMessage response)
            where T : HttpRequestResult, new()
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            T result;

            if (jsonString != string.Empty)
            {
                
                result = JsonConvert.DeserializeObject<T>(jsonString);
            }
            else
            {
                result = (T)Convert.ChangeType(new T(), typeof(T));
            }

            result.RawResponse = jsonString;

            result.StatusCode = response.StatusCode;

            result.UpdateFromHeaders(response.Headers);

            result.Success = response.IsSuccessStatusCode;

            return result;
        }

        public static async Task<IEnumerable<T>> ParseMultipleAsync<T>(HttpResponseMessage response)
            where T : HttpRequestResult, new()
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new List<T> { new T() { Success = false, RawResponse = jsonString, StatusCode = response.StatusCode } };
            }
            var results = JsonConvert.DeserializeObject<IEnumerable<T>>(jsonString);

            foreach (var result in results)
            {
                result.Success = true;
                result.UpdateFromHeaders(response.Headers);
            }

            return results;
        }

        public static async Task<IEnumerable<T>> ParseMultipleNestedAsync<T>(HttpResponseMessage response, string root)
            where T : HttpRequestResult, new()
        {
            var jsonString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return new List<T> { new T() { Success = false, RawResponse = jsonString, StatusCode = response.StatusCode } };
            }
            var results = JsonConvert.DeserializeObject<IEnumerable<T>>(JObject.Parse(jsonString).SelectToken(root).ToString());

            foreach (var result in results)
            {
                result.Success = true;
                result.UpdateFromHeaders(response.Headers);
            }

            return results;
        }

        protected virtual void UpdateFromHeaders(HttpHeaders headers)
        {
            if (headers.TryGetValues(RequestIdKey, out var values))
            {
                this.RequestId = Guid.Parse(values.First());
            }
        }
    }
}