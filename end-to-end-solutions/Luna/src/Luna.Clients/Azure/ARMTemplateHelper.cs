// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Luna.Clients.Azure
{
    public class ARMTemplateHelper
    {
        /// <summary>
        /// Get all parameters and type from arm template
        /// </summary>
        /// <param name="armTemplateContent">The json content of the template</param>
        /// <returns>The name:type key value pairs</returns>
        public static List<KeyValuePair<string, string>> GetArmTemplateParameters(string armTemplateContent)
        {
            try
            {
                List<KeyValuePair<string, string>> parameters = new List<KeyValuePair<string, string>>();
                dynamic data = JsonConvert.DeserializeObject(armTemplateContent);
                foreach (dynamic param in data.parameters)
                {
                    string name = param.Name;
                    string type = param.Value.type;
                    parameters.Add(new KeyValuePair<string, string>(name, type));
                }

                return parameters;
            }
            catch(NullReferenceException)
            {
                // If the property of the dynamic object doesn't exist, it will throw NullReferenceException
                // Catch and rethrow a ArgumentException with better message here.
                // TODO: review error message.
                throw new ArgumentException("The ARM template doesn't contain Parameters object.");
            }
        }
    }
}
