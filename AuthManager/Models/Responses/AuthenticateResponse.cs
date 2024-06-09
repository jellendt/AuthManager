using AuthManager.Entities;
using System.Text.Json.Serialization;

namespace AuthManager.Models.Responses
{
    public class AuthenticateResponse(string username, string eMail, string jwtToken)
    {
        public string Username { get; set; } = username;
        public string EMail { get; set; } = eMail;
        public string JwtToken { get; set; } = jwtToken;

    }
}
