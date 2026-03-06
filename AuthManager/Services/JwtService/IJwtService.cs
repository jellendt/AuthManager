using AuthManager.DependecyInjection;
using AuthManager.Entities;
using System.Security.Claims;

namespace AuthManager.Services.JwtService
{
    public interface IJwtService : IScopedDependency
    {
        string GenerateJwtToken(User user);
        Guid? GetGuidIdFromClaims(List<Claim> claims);
    }
}
