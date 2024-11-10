using AuthManager.Entities;
using AuthManager.Models.Requests;

namespace AuthManager.Services.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<User> Register(RegisterRequest registerRequest);
        Task<User?> Login(LoginRequest loginRequest);
        Task<(string jwtToken, RefreshToken refreshToken)?> Refresh(string userRefreshToken);
        Task<(User user, RefreshToken refreshToken)?> LoginWithRefresh(string userRefreshToken);
    }
}