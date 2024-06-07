namespace AuthManager.Models
{
    public class RegisterRequest(string username, string password, string eMail)
    {
        public required string Username { get; set; } = username;
        public required string Password { get; set; } = password;
        public required string EMail { get; set; } = eMail;
    }
}
