using System;
using System.Net;

namespace Luna.Clients.Exceptions
{
    public class LunaUserException: Exception
    {
        public UserErrorCode Code { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        public string Target { get; set; }

        public LunaUserException(string message, UserErrorCode code, HttpStatusCode statusCode, string target = "method_error"): base(message)
        {
            this.Code = code;
            this.HttpStatusCode = statusCode;
            this.Target = target;
        }
    }
}
