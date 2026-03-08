using AuthManager.DependecyInjection;
using AuthManager.Entities;
using AuthManager.Models.Requests;

namespace AuthManager.Services.AuthenticationService
{
    public interface ITokenService : IScopedDependency
    {
        Task<(string jwtToken, RefreshToken refreshToken)> Register(RegisterRequest registerRequest);
        Task<(string jwtToken, RefreshToken refreshToken)> Login(LoginRequest loginRequest);
        Task<(string jwtToken, RefreshToken refreshToken)> Refresh(string userRefreshToken);
    }
}