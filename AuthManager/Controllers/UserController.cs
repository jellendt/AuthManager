using AuthManager.Entities;
using AuthManager.Exceptions;
using AuthManager.Models.Requests;
using AuthManager.Services.AuthenticationService;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthManager.Controllers
{
    [Authorize]
    [ApiController]
    [Route("v1/user")]
    public class UserController
        (
            [FromServices] IUserService userService,
            [FromServices] IJwtService jwtService
        ) : Controller
    {
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] UserChangeRequest userChangeRequest)
        {
            Guid? userGuid = this._jwtService.GetGuidIdFromClaims(this.HttpContext.User.Claims.ToList());

            if(userGuid == null)
                return this.Unauthorized();

            User user = await _userService.GetByGuid((Guid)userGuid) ?? throw new UserNotFoundException();

            user.EMail = userChangeRequest.EMail;

            return this.Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete()
        {
            Guid? userGuid = this._jwtService.GetGuidIdFromClaims(this.HttpContext.User.Claims.ToList());
            if (userGuid == null)
                return this.Unauthorized();

            await _userService.Delete((Guid)userGuid);
            return this.Ok();
        }
    }
}
