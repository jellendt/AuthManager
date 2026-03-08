using AuthManager.DependecyInjection;
using AuthManager.Entities;

namespace AuthManager.Services.JwtService
{
    public interface IJwtService : IScopedDependency
    {
        string GenerateJwtToken(User user);
    }
}
