using AuthManager.Entities;
using System.Text.Json.Serialization;

namespace AuthManager.Models
{
    public class AuthenticateResponse(User user, string jwtToken, string refreshToken)
    {
        public Guid Id { get; set; } = user.Id;
        public string Username { get; set; } = user.Username;
        public string JwtToken { get; set; } = jwtToken;

        [JsonIgnore] // refresh token is returned in http only cookie
        public string RefreshToken { get; set; } = refreshToken;
    }
}
