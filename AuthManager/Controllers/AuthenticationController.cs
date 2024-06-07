using AuthManager.Entities;
using AuthManager.Models;
using AuthManager.Services.AuthenticationService;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace AuthManager.Controllers
{
    [ApiController]
    [Route("authentication")]
    public class AuthenticationController(
            [FromServices] IAuthenticationService authenticationService,
            [FromServices] IUserService userService
            ) : Controller
    {
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly IUserService _userService = userService;

        [HttpPut("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
        {
            User user = await this._authenticationService.Register(registerRequest);
            if (user.ActiveRefreshToken == null)
                return this.Unauthorized();
            this.SetRefreshToken(user.ActiveRefreshToken);
            return this.Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            //string jwtToken = await this._authenticationService.Login(loginRequest);
            //if(string.IsNullOrEmpty(jwtToken))
            //    return this.Unauthorized();
            //return this.Ok();
            return this.Ok();
        }

        [HttpGet("test")]
        [Authorize]
        public IActionResult Test()
        {
            return this.Ok("Ja ist Ok");
            //return this.Ok(this._authenticationService.Ver());
        }

        private void SetRefreshToken(RefreshToken refreshToken)
        {
            this.Response.Cookies.Append("accessToken", refreshToken.Token,
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
