using AuthManager.Contexts;
using AuthManager.Entities;
using AuthManager.Models.Requests;
using AuthManager.Services.JwtService;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AuthManager.Services.AuthenticationService
{
    public partial class TokenService(
        [FromServices] IUserService userService,
        [FromServices] IJwtService jwtService,
        [FromServices] IAuthenticationService authenticationService,
        [FromServices] DbAuthContext dbAuthContext
         ) : ITokenService
    {
        public async Task<(string jwtToken, RefreshToken refreshToken)?> Register(RegisterRequest registerRequest)
        {
            await CheckUserRequirements(registerRequest);

            User newUser = authenticationService.CreateUser(registerRequest.Username, registerRequest.Password, registerRequest.EMail);
            RefreshToken refreshToken = GenerateRefreshToken();
            newUser.RefreshTokens.Add(refreshToken);
            User user = await userService.Add(newUser);

            return (jwtService.GenerateJwtToken(user), refreshToken);
        }

        private async Task CheckUserRequirements(RegisterRequest registerRequest)
        {
            if (!EMail().IsMatch(registerRequest.EMail))
                throw new Exception("Bitte geb eine Valide E-Mail an!");

            Task<bool> emailExistsTask = userService.CheckEmailExists(registerRequest.EMail);
            Task<bool> usernameExistsTask = userService.CheckUsernameExists(registerRequest.Username);

            if (await emailExistsTask)
                throw new Exception("Diese E-Mail wurde bereits verwendet!");

            if (await usernameExistsTask)
                throw new Exception("Dieser Username wurde bereits verwendet!");
        }

        public async Task<(string jwtToken, RefreshToken refreshToken)?> Login(LoginRequest loginRequest)
        {
            User? user = await userService.GetByEmail(loginRequest.EMail);
            if (user == null)
                return null;

            if (!authenticationService.CheckPassword(user, loginRequest.Password))
                return null;

            RefreshToken newRefreshToken = GenerateRefreshToken();
            foreach (RefreshToken refreshToken in user.RefreshTokens.Where(u => u.IsActive))
            {
                refreshToken.ReplacedByToken = newRefreshToken.Id;
                refreshToken.Expires = DateTime.UtcNow;
                refreshToken.ReasonRevoked = "revoked by system";
            }
            user.RefreshTokens.Add(newRefreshToken);
            await dbAuthContext.SaveChangesAsync();
            string jwtToken = jwtService.GenerateJwtToken(user);
            return (jwtToken, newRefreshToken);

        }

        public async Task<(string jwtToken, RefreshToken refreshToken)?> Refresh(string userRefreshToken)
        {
            RefreshToken? oldRefreshToken = await dbAuthContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token.Equals(userRefreshToken) && rt.Revoked == null && DateTime.UtcNow < rt.Expires);

            if (oldRefreshToken == null)
                return null;

            User? user = await dbAuthContext.Users.FirstOrDefaultAsync(u => u.Id == oldRefreshToken.UserId);

            if (user == null)
                return null;

            string jwtToken = jwtService.GenerateJwtToken(user);
            RefreshToken newRefreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(newRefreshToken);
            dbAuthContext.Add(newRefreshToken);

            oldRefreshToken.ReplacedByToken = newRefreshToken.Id;
            oldRefreshToken.Revoked = DateTime.UtcNow;
            oldRefreshToken.ReasonRevoked = "revoked by system";

            await dbAuthContext.SaveChangesAsync();

            return (jwtToken, newRefreshToken);

        }

        private static RefreshToken GenerateRefreshToken()
        {
            byte[] randomBytes = new byte[64];
            RandomNumberGenerator.Fill(randomBytes);
            RefreshToken refreshToken = new()
            {
                Token = Convert.ToBase64String(randomBytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
            };

            return refreshToken;
        }

        [GeneratedRegex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$")]
        public static partial Regex EMail();
    }
}
