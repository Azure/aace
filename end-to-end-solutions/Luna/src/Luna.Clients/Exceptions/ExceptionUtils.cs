using System;
using System.Net;

namespace Luna.Clients.Exceptions
{
    public class ExceptionUtils
    {
        /// <summary>
        /// Check if the error code is retryable. Only retry on 500, 502, 503 and 504
        /// </summary>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public static bool IsHttpErrorCodeRetryable(HttpStatusCode errorCode)
        {
            return errorCode == HttpStatusCode.InternalServerError ||
                errorCode == HttpStatusCode.BadGateway ||
                errorCode == HttpStatusCode.ServiceUnavailable ||
                errorCode == HttpStatusCode.GatewayTimeout;

        }

        public static string GetFormattedDetails(Exception ex)
        {
            string thisException = string.Format("{0}: {1} - {2}", ex.GetType().Name, ex.Message, ex.StackTrace);

            if (ex.InnerException != null)
            {
                // Recursively get inner exception details
                string innerExceptionDetails = GetFormattedDetails(ex.InnerException);

                if (!string.IsNullOrEmpty(innerExceptionDetails))
                {
                    thisException = string.Format("{0}\n\nINNER: {1}", thisException, innerExceptionDetails);
                }

            }
            return thisException;
        }
    }
}
