
namespace AuthManager.Models.Responses
{
    public class UserResponse(string username, string eMail)
    {
        public string Username { get; set; } = username;
        public string EMail { get; set; } = eMail;

    }
}
