namespace AuthManager.Exceptions.Registration
{
    public class UsernameInUseException : ApiException
    {
        public UsernameInUseException() : base(StatusCodes.Status409Conflict, "Username already in use")
        {
        }
    }
}
