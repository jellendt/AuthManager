using AuthManager.Entities;
using AuthManager.Extensions;
using AuthManager.Models.Requests;
using AuthManager.Models.Responses;
using AuthManager.Services.AuthenticationService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthManager.Controllers
{
    [ApiController]
    [Route("v1/authentication")]
    public class AuthenticationController([FromServices] ITokenService authenticationService)
        : Controller
    {
        private readonly ITokenService _authenticationService = authenticationService;

        [HttpPut("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            (string jwtToken, RefreshToken refreshToken) authCredentials =
                await this._authenticationService.Register(registerRequest);
            this.SetRefreshToken(authCredentials.refreshToken);
            return this.Ok(ConvertToTokenResponse(authCredentials));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            (string jwtToken, RefreshToken refreshToken) authCredentials =
                await this._authenticationService.Login(loginRequest);
            this.SetRefreshToken(authCredentials.refreshToken);

            return this.Ok(ConvertToTokenResponse(authCredentials));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!this.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                return this.Unauthorized();

            (string jwtToken, RefreshToken refreshToken) authCredentials =
                await this._authenticationService.Refresh(refreshToken);
            this.SetRefreshToken(authCredentials.refreshToken);

            return this.Ok(ConvertToTokenResponse(authCredentials));
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            Guid? userGuid = HttpContext.GetUserIdFromClaims();
            return this.Ok(userGuid);
        }

        private void SetRefreshToken(RefreshToken refreshToken)
        {
            this.Response.Cookies.Append(
                "refreshToken",
                refreshToken.Token,
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.Add(refreshToken.Expires - DateTime.Now),
                    HttpOnly = true,
                    IsEssential = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                }
            );
        }

        private static TokenResponse ConvertToTokenResponse(
            (string jwtToken, RefreshToken refreshToken) authCredentials
        )
        {
            return new TokenResponse()
            {
                Token = authCredentials.jwtToken,
                ValidUntil = authCredentials.refreshToken.Expires,
            };
        }
    }
}
