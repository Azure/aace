using System;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Luna.Clients.Logging
{
    public class LoggingUtils
    {
        private static string AppendParentResourceNames(
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            if (!string.IsNullOrEmpty(planName))
            {
                message.Append($" in plan {planName}");
            }
            if (!string.IsNullOrEmpty(offerName))
            {
                message.Append($" in offer {offerName}");
            }
            if (subscriptionId != null)
            {
                message.Append($" in subscription {subscriptionId}");
            }

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of return value
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="value">The value</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeReturnValueMessage(string resourceType,
            string resourceName,
            string value,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($" returned with value {value}.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of number of returned resources
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="count">The number of resources returned</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeReturnCountMessage(string resourceType,
            int count,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{count} resources with type {resourceType} ");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($" returned.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of getting a single resource
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeGetSingleResourceMessage(string resourceType, string resourceName, string planName = "", string offerName = "", Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Get {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($".");

            return message.ToString();
        }


        /// <summary>
        /// Compose a message of getting all resources
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeGetAllResourcesMessage(string resourceType, string planName = "", string offerName = "", Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Get all {resourceType}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($".");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of a single resource exists or not
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="resourceExists">The resource exists or not</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeResourceExistsOrNotMessage(string resourceType,
            string resourceName,
            bool resourceExists,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            if (resourceExists)
            {
                message.Append($"exists.");
            }
            else
            {
                message.Append($"doesn't exist.");
            }

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of checking a single resource exists or not
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeCheckResourceExistsMessage(string resourceType,
            string resourceName,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Check if {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($"exists.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of deleting a single resource
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeDeleteResourceMessage(string resourceType,
            string resourceName,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Delete {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($".");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of deleting a single resource
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="payload">The payload string</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeCreateResourceMessage(string resourceType,
            string resourceName,
            string payload = "",
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Create {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            if (!string.IsNullOrEmpty(payload))
            {
                message.Append($" with payload {payload}");
            }
            message.Append($".");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of update a single resource
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="payload">The payload string</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeUpdateResourceMessage(string resourceType,
            string resourceName,
            string payload = "",
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Update {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            if (!string.IsNullOrEmpty(payload))
            {
                message.Append($" with payload {payload}");
            }
            message.Append($".");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of a  resource deleted
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeResourceDeletedMessage(string resourceType,
            string resourceName,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($" is deleted.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of a  resource created
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeResourceCreatedMessage(string resourceType,
            string resourceName,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($" is created.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message of a  resource updated
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeResourceUpdatedMessage(string resourceType,
            string resourceName,
            string planName = "",
            string offerName = "",
            Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($" is updated.");

            return message.ToString();
        }

        /// <summary>
        /// Compose an error message showing resource doesn't exist
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The error message</returns>
        public static string ComposeNotFoundErrorMessage(string resourceType, string resourceName, string planName = "", string offerName = "", Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with {resourceName} ");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($"doesn't exist.");

            return message.ToString();
        }

        /// <summary>
        /// Compose error message of payload not provided
        /// </summary>
        /// <param name="payloadName">The payload name</param>
        /// <returns>The error message</returns>
        public static string ComposePayloadNotProvidedErrorMessage(string payloadName)
        {
            return $"The {payloadName} payload is not provided.";
        }

        /// <summary>
        /// Compose error message of name mismatch
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <returns>The error message</returns>
        public static string ComposeNameMismatchErrorMessage(string resourceType)
        {
            return $"The {resourceType} name in url doesn't match the one in request body.";
        }

        /// <summary>
        /// Compose an error message of resource already exists
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The error message</returns>
        public static string ComposeAlreadyExistsErrorMessage(string resourceType, string resourceName, string planName = "", string offerName = "", Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{resourceType} with name {resourceName} ");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($"already exists.");

            return message.ToString();
        }

        /// <summary>
        /// Compose error message of parameter name is reserved
        /// </summary>
        /// <param name="parameterName">The parameter name</param>
        /// <returns>The error message</returns>
        public static string ComposeParameterNameReservedErrorMessage(string parameterName)
        {
            return $"The parameter name {parameterName} is reserved. Please use a different name.";
        }

        /// <summary>
        /// Compose a message of found duplicated instances
        /// </summary>
        /// <param name="resourceType">The resource type</param>
        /// <param name="resourceName">The resource name</param>
        /// <param name="planName">The plan name</param>
        /// <param name="offerName">The offer name</param>
        /// <param name="subscriptionId">The subscription id</param>
        /// <returns>The log message</returns>
        public static string ComposeFoundDuplicatesErrorMessage(string resourceType, string resourceName, string planName = "", string offerName = "", Guid? subscriptionId = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"Found multiple instances of {resourceType} with name {resourceName}");
            message.Append(AppendParentResourceNames(planName, offerName, subscriptionId));
            message.Append($". Only one record should be found.");

            return message.ToString();
        }

        /// <summary>
        /// Compose a message for logging outgoing http requests 
        /// </summary>
        /// <param name="requestId">The request id</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="caller">The caller</param>
        /// <param name="method">The http method</param>
        /// <param name="requestUri">The uri</param>
        /// <param name="content">The request content</param>
        /// <returns></returns>
        public static string ComposeHttpRequestLogMessage(Guid requestId, Guid correlationId, string caller, HttpMethod method, Uri requestUri, string content)
        {
            if (String.IsNullOrEmpty(content))
            {
                return $"Sending request {caller}, method:{method}, requestUri: {requestUri} requestId: {requestId} correlationId: {correlationId}";

            } else
            {
                return $"Sending request {caller}, method:{method}, requestUri: {requestUri} requestId: {requestId} correlationId: {correlationId}, content:{content}";

            }
        }

        /// <summary>
        /// Compose a message for logging http responses
        /// </summary>
        /// <param name="requestId">The request id</param>
        /// <param name="correlationId">The correlation id</param>
        /// <param name="caller">The caller</param>
        /// <param name="responseStatusCode">The response status code</param>
        /// <param name="result">The response content</param>
        /// <returns></returns>
        public static string ComposeHttpResponseLogMessage(Guid requestId, Guid correlationId, string caller, HttpStatusCode? responseStatusCode = null, string result = null, string requestUri = null)
        {
            if (responseStatusCode == null && String.IsNullOrEmpty(result))
            {
                return $"No response {caller}: requestId: {requestId} correlationId: {correlationId}. Request uri: {requestUri}";
            }
            return
                $"Received response {caller}: requestId: {requestId} correlationId: {correlationId}. Status: {responseStatusCode}. Response content: {result}";
        }

        public static string ComposeSubscriptionActionMessage(string action, Guid subscriptionId, string planId = null, string quantity = null, string actionBy = null)
        {
            StringBuilder message = new StringBuilder();
            message.Append($"{action} subscription {subscriptionId}");
            if (!String.IsNullOrEmpty(planId))
            {
                message.Append($" for plan {planId}");
            }
            if (!String.IsNullOrEmpty(quantity))
            {
                message.Append($" with quantity {quantity}");
            }
            if (!String.IsNullOrEmpty(actionBy))
            {
                message.Append($" by {actionBy}");
            }
            return message.Append(".").ToString();
        }

        public static string ComposeBadXorArgumentMessage(string property1, string property2)
        {
            return $"Use either the {property1} property or the {property2} property, but not both.";
        }

        public static string ComposeHttpClientLogMessage(string client, string method, Guid? subscriptionId = null)
        {
            return $"Executing {client} method {method} for subscription {subscriptionId}.";
        }

        public static string ComposeSubscriptionActionErrorMessage(Guid subscriptionId, string action, bool invalidProvisioningState = false, string requiredFulfillmentState = null, string invalidFulfillmentState = null)
        {
            if (invalidProvisioningState && !(String.IsNullOrEmpty(requiredFulfillmentState) && String.IsNullOrEmpty(invalidFulfillmentState)))
            {
                throw new ArgumentException("Cannot compose single error message for both provisioning state and fulfillment state errors.");
            }

            if (!String.IsNullOrEmpty(requiredFulfillmentState) && !String.IsNullOrEmpty(invalidFulfillmentState))
            {
                throw new ArgumentException("Cannot compose single error message for both required and invalid fulfillment states.");
            }

            StringBuilder message = new StringBuilder();
            message.Append($"Cannot {action} subscription {subscriptionId} ");

            if (invalidProvisioningState)
            {
                message.Append("with pending or failed provisioning operations.");
                return message.ToString();
            }

            if (!String.IsNullOrEmpty(requiredFulfillmentState))
            {
                message.Append($"that is not in the {requiredFulfillmentState} state.");
                return message.ToString();
            }

            if (!String.IsNullOrEmpty(invalidFulfillmentState))
            {
                message.Append($"in {invalidFulfillmentState} state.");
                return message.ToString();
            }

            return message.ToString().Trim();
        }
    }

}
