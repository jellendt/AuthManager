namespace AuthManager.Exceptions
{
    public class ApiException(int statusCode, string message) : Exception(message)
    {
        public int StatusCode { get; } = statusCode;
    }
}
