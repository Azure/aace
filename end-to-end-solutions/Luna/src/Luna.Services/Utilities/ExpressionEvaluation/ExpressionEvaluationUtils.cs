// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Luna.Services.Utilities.ExpressionEvaluation
{
    /// <summary>
    /// Utilities to evaluate c# expression for parameters
    /// </summary>
    public class ExpressionEvaluationUtils
    {
        static public string OfferNameParameterName = "system$$offerName";
        static public string SubscriptionOwnerParameterName = "system$$subscriptionOwner";
        static public string SubscriptionIdParameterName = "system$$subscriptionId";
        static public string PlanNameParameterName = "system$$planName";
        static public string OperationTypeParameterName = "system$$operationType";

        //TODO: should put the list in some better place. Database?
        static public string[] ReservedParameterNames = new string[] { 
            OfferNameParameterName, 
            SubscriptionOwnerParameterName, 
            SubscriptionIdParameterName,
            PlanNameParameterName, 
            OperationTypeParameterName
        };
        /// <summary>
        /// The evaluation context
        /// </summary>
        public Context Context { get; set; }

        /// <summary>
        /// ctor.
        /// </summary>
        /// <param name="context"></param>
        public ExpressionEvaluationUtils(Context context)
        {
            this.Context = context;
        }

        /// <summary>
        /// Evaluate a single expression
        /// </summary>
        /// <param name="name">The expression name. Only being used when addToContext == true</param>
        /// <param name="expression">The c# expression</param>
        /// <param name="addToContext">Specify if add this parameter to the current context</param>
        /// <returns></returns>
        public async Task<object> Evaluate(string name, string expression, bool addToContext = false)
        {
            try
            {
                object result= await CSharpScript.EvaluateAsync(expression, globals: this.Context, 
                    options: ScriptOptions.Default.WithReferences(typeof(Context).Assembly).AddImports("Luna.Services.Utilities.ExpressionEvaluation"));

                if (addToContext && !Context.Parameters.ContainsKey(name))
                {
                    this.Context.Parameters.Add(name, result);
                }

                return result;
            }
            catch (CompilationErrorException e)
            {
                throw new ArgumentException($"Can not evaluate expression {expression} for parameter {name}. Error: {e.Message}.");
            }
        }

        /// <summary>
        /// Evaluate a list of parameters
        /// This function will resolve the dependencies and sort the parameters. 
        /// </summary>
        /// <param name="parameterList">The parameter list dictionary</param>
        /// <returns></returns>
        public async Task EvaluateAll(Dictionary<string, string> parameterList)
        {
            ArrayList sortedParamList = SortParametersForEstimation(parameterList);
            foreach (string param in sortedParamList)
            {
                await Evaluate(param, parameterList[param], true);
            }
        }

        /// <summary>
        /// Sort the parameters by dependencies. 
        /// Will also detect circular dependencies and throw exception
        /// </summary>
        /// <param name="parameterList">the parameter list as dictionary</param>
        /// <returns></returns>
        private ArrayList SortParametersForEstimation(Dictionary<string, string> parameterList)
        {
            List<KeyValuePair<string, string>> dependencyMap = BuildDependencyMap(parameterList);

            ArrayList keys = new ArrayList();
            while(dependencyMap.Count >0)
            {
                List<string> allKeys = new List<string>();
                allKeys.AddRange(parameterList.Keys);
                foreach (var dependency in dependencyMap)
                {
                    if (allKeys.Contains(dependency.Key))
                    {
                        allKeys.Remove(dependency.Key);
                    }
                }

                // detected circular references
                if (allKeys.Count == 0)
                {
                    // Review error message
                    throw new ArgumentException("Circular reference is detected in the parameter list. Please check the parameter value definition and remove circular expression.");
                }

                List<KeyValuePair<string, string>> newDependencyMap = new List<KeyValuePair<string, string>>();
                foreach (var dependency in dependencyMap)
                {
                    if (!allKeys.Contains(dependency.Value))
                    {
                        newDependencyMap.Add(dependency);
                    }
                }
                foreach (var key in allKeys)
                {
                    if(!keys.Contains(key))
                    {
                        keys.Add(key);
                    }
                }
                dependencyMap = newDependencyMap;
            }

            foreach(var key in parameterList.Keys)
            {
                if (!keys.Contains(key))
                {
                    keys.Add(key);
                }
            }

            return keys;

        }

        /// <summary>
        /// Build the dependency map between parameters
        /// Parameter A depends on B if A = ...Parameters["B"]...
        /// </summary>
        /// <param name="parameterList">The parameter list as dictionary</param>
        /// <returns></returns>
        private List<KeyValuePair<string, string>> BuildDependencyMap(Dictionary<string, string> parameterList)
        {
            List<KeyValuePair<string, string>> dependencyMap = new List<KeyValuePair<string, string>>();
            foreach (var key in parameterList.Keys)
            {
                foreach (var dependentKey in parameterList.Keys)
                {
                    if (parameterList[dependentKey].Contains(string.Format("Parameters[\"{0}\"]", key)))
                    {
                        dependencyMap.Add(new KeyValuePair<string, string>(dependentKey, key));
                    }
                }
            }
            return dependencyMap;
        }
    }
}
