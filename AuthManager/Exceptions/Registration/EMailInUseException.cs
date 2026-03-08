namespace AuthManager.Exceptions.Registration
{
    public class EMailInUseException : ApiException
    {
        public EMailInUseException() : base(StatusCodes.Status409Conflict, "Email already in use")
        {

        }
    }
}
