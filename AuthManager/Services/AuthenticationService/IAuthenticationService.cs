using AuthManager.Entities;
using AuthManager.Models;

namespace AuthManager.Services.AuthenticationService
{
    public interface IAuthenticationService
    {
        Task<User> Register(RegisterRequest registerRequest);
        Task<User?> Login(LoginRequest loginRequest);
    }
}