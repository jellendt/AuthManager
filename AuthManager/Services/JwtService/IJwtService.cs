using AuthManager.Entities;

namespace AuthManager.Services.AuthenticationService
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
        Guid? ValidateJwtToken(string token);
    }
}
