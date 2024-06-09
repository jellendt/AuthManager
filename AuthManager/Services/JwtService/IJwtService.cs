using AuthManager.Entities;
using System.Security.Claims;

namespace AuthManager.Services.AuthenticationService
{
    public interface IJwtService
    {
        string GenerateJwtToken(User user);
        //Guid? GetGuidFromJwtToken(string token);
        Guid? GetGuidIdFromClaims(List<Claim> claims);
    }
}
