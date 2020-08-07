// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Luna.Clients.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Luna.Clients.Models
{
    /* 
      https://github.com/Microsoft/api-guidelines/blob/master/Guidelines.md#710-response-formats 
    */

    internal class LowercaseContractResolver : DefaultContractResolver
    {
        protected override string ResolvePropertyName(string propertyName)
        {
            return propertyName.ToLower();
        }
    }

    public class ErrorModel
    {
        public ErrorModel(System.Exception exception)
        {
            if (exception.GetType().Name.Equals(nameof(LunaUserException)) || exception.GetType().BaseType.Name.Equals(nameof(LunaUserException)))
            {
                this.Error = new ExceptionModel(((LunaUserException)exception).Code.ToString(), exception.Message, ((LunaUserException)exception).Target);
                this.Error.Details = new List<ExceptionModel>();
                this.Error.Details.Add(new ExceptionModel(((LunaUserException)exception).Code.ToString(), exception.Message, ((LunaUserException)exception).Target));
            }
            else
            {
                this.Error = new ExceptionModel("InternalServerError", "The service is temporarily unavailable.");
                this.Error.Details = new List<ExceptionModel>();
                this.Error.Details.Add(new ExceptionModel("InternalServerError", "The service is temporarily unavailable."));
            }
            
            if (exception.InnerException != null)
            {
                this.Error.InnerError = new InnerException(exception.InnerException);
            }
        }

        public ErrorModel(ActionContext context)
        {
            this.Error = new ExceptionModel("BadRequest", "The inputs are invalid.");
            this.Error.Details = new List<ExceptionModel>();

            foreach (var keyModelStatePair in context.ModelState)
            {
                var key = keyModelStatePair.Key;
                var errors = keyModelStatePair.Value.Errors;
                if (errors != null && errors.Count > 0)
                {
                    this.Error.Details.Add(new ExceptionModel("BadRequest", string.IsNullOrEmpty(errors[0].ErrorMessage) ? "The input was invalid." : errors[0].ErrorMessage, key));
                }
            }
        }

        public ExceptionModel Error { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.None, new JsonSerializerSettings() { ContractResolver = new LowercaseContractResolver() });
        }
    }

    public class ExceptionModel
    {
        public ExceptionModel(string code, string message, string target = "method_error")
        {
            this.Code = code;
            this.Message = message;
            this.Target = target;
        }

        [Required]
        public string Code { get; set; }

        [Required]
        public string Message { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Target { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<ExceptionModel> Details { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public InnerException InnerError { get; set; }
    }

    public class InnerException
    {
        public InnerException(System.Exception exception)
        {
            this.Code = exception.GetType().Name;
            if (exception.InnerException != null)
            {
                this.InnerError = new InnerException(exception.InnerException);
            }
        }

        [Required]
        public string Code { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public InnerException InnerError { get; set; }
    }
}
