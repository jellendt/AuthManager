using AuthManager.Entities;
using AuthManager.Models.Requests;
using AuthManager.Models.Responses;
using AuthManager.Services.AuthenticationService;
using AuthManager.Services.UserService;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthManager.Controllers
{
    [ApiController]
    [Route("v1/authentication")]
    public class AuthenticationController(
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] IJwtService jwtService
            ) : Controller
    {
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly IJwtService _jwtService = jwtService;

        [HttpPut("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            (string jwtToken, RefreshToken refreshToken)? authCredentials = await this._authenticationService.Register(registerRequest);
            if (authCredentials == null || authCredentials.Value.refreshToken == null)
                return this.Unauthorized();

            this.SetRefreshToken(authCredentials.Value.refreshToken);

            return this.Ok(authCredentials.Value.jwtToken);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {

            (string jwtToken, RefreshToken refreshToken)? authCredentials = await this._authenticationService.Login(loginRequest);
            if (authCredentials == null || authCredentials.Value.refreshToken == null)
                return this.Unauthorized();

            this.SetRefreshToken(authCredentials.Value.refreshToken);

            return this.Ok(authCredentials.Value.jwtToken);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!this.Request.Cookies.TryGetValue("refreshToken", out string? refreshToken))
                return this.Unauthorized();

            (string jwtToken, RefreshToken refreshToken)? tokens = await this._authenticationService.Refresh(refreshToken);
            if (tokens == null)
                return this.Unauthorized();
            this.SetRefreshToken(tokens.Value.refreshToken);

            return this.Ok(tokens.Value.jwtToken);
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            Guid? userGuid = this._jwtService.GetGuidIdFromClaims(this.HttpContext.User.Claims.ToList());
            //User user = _jwtService.GetGuidFromJwtToken()
            return this.Ok(userGuid);
            //return this.Ok(this._authenticationService.Ver());
        }

        private void SetRefreshToken(RefreshToken refreshToken)
        {
            this.Response.Cookies.Append("refreshToken", refreshToken.Token,
                        new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.Add(refreshToken.Expires - DateTime.Now),
                            HttpOnly = true,
                            IsEssential = true,
                            Secure = true,
                            SameSite = SameSiteMode.None
                        });
        }

    }
}
