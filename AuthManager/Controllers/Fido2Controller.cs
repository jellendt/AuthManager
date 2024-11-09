using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Fido2NetLib.Objects;
using AuthManager.Services.UserService;
using AuthManager.Entities;

namespace AuthManager.Controllers
{
    [ApiController]
    [Route("v1/fido2")]
    public class Fido2Controller(
            [FromServices] UserService userService
        ) : Controller
    {
        private readonly UserService _userService = userService;
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> CreateCredentialOptions([FromBody] string eMail)
        {
            if (string.IsNullOrWhiteSpace(eMail))
                return this.BadRequest("Email is requierd");

            User? user = await _userService.GetByEmail(eMail);
            if (user is not null)
                return this.BadRequest($"Email {eMail} is already in use!");

            List<FidoCredential> fidoCredentials = user.Credentials;


        }
    }
}
