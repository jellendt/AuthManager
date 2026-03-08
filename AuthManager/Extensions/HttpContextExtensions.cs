using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthManager.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid? GetUserIdFromClaims(this HttpContext httpContext)
        {
            if (httpContext.User.Identity is not { IsAuthenticated: true })
                return null;

            List<Claim> claims = [.. httpContext.User.Claims];
            if (Guid.TryParse(claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value, out Guid userGuid))
            {
                return userGuid;
            }
            return null;
        }
    }
}
