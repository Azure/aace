namespace Luna.Clients.Exceptions
{
    public class LunaBadRequestUserException : LunaUserException
    {
        public LunaBadRequestUserException(string message, UserErrorCode code):base(message, code, System.Net.HttpStatusCode.BadRequest)
        {

        }
    }
}
