namespace AuthManager.Exceptions.Registration
{
    public class InvalidEMailException : ApiException
    {
        public InvalidEMailException() : base(StatusCodes.Status409Conflict, "Invalid EMail")
        {
        }
    }
}
