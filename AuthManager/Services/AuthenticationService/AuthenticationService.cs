using AuthManager.Contexts;
using AuthManager.Entities;
using AuthManager.Models.Requests;
using AuthManager.Regexes;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace AuthManager.Services.AuthenticationService
{
    public partial class AuthenticationService(
        [FromServices] IUserService userService,
        [FromServices] IJwtService jwtService,
        [FromServices] DbAuthContext dbAuthContext
         ): IAuthenticationService
    {
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly DbAuthContext _dbAuthContext = dbAuthContext;

        public async Task<(string jwtToken, RefreshToken refreshToken)?> Register(RegisterRequest registerRequest)
        {

            if (!Matcher.EMail().IsMatch(registerRequest.EMail))
                throw new Exception("Bitte geb eine Valide E-Mail an!");

            if((await this._userService.GetByEmail(registerRequest.EMail)) != null)
                throw new Exception("Diese E-Mail wurde bereits verwendet!");

            if ((await this._userService.GetByEmail(registerRequest.Username)) != null)
                throw new Exception("Dieser Username wurde bereits verwendet!");

            (string hash, string salt) = PasswordHasher.HashPassword(registerRequest.Password);
            RefreshToken refreshToken = GenerateRefreshToken();

            List<RefreshToken> refreshTokens = [refreshToken];
            User newUser = new()
            {
                Id = Guid.NewGuid(),
                Username = registerRequest.Username,
                PasswordHash = hash,
                Salt = salt,
                EMail = registerRequest.EMail,
                Role = Enum.RoleEnum.User,
                RefreshTokens = refreshTokens
            };

            User user = await this._userService.Add(newUser);

            return (this._jwtService.GenerateJwtToken(user), refreshToken);
        }

        public async Task<(string jwtToken, RefreshToken refreshToken)?> Login(LoginRequest loginRequest)
        {
            User? user = await this._userService.GetByEmail(loginRequest.EMail);
            if (user == null)
                return null;

            if(PasswordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash, user.Salt))
            {
                RefreshToken newRefreshToken = GenerateRefreshToken();
                
                foreach(RefreshToken refreshToken in user.RefreshTokens.Where(u => u.IsActive))
                {
                    refreshToken.ReplacedByToken = newRefreshToken.Id;
                    refreshToken.Expires = DateTime.UtcNow;
                    refreshToken.ReasonRevoked = "revoked by system";
                }
                user.RefreshTokens.Add(newRefreshToken);
                await this._dbAuthContext.SaveChangesAsync();
                return (this._jwtService.GenerateJwtToken(user), newRefreshToken);
            }

            return null;
        }

        public async Task<(string jwtToken, RefreshToken refreshToken)?> Refresh(string userRefreshToken)
        {
            RefreshToken? oldRefreshToken = await this._dbAuthContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token.Equals(userRefreshToken) && rt.Revoked == null && DateTime.UtcNow < rt.Expires);

            if (oldRefreshToken == null)
                return null;

            User? user = await this._dbAuthContext.Users.FirstOrDefaultAsync(u => u.Id == oldRefreshToken.UserId);

            if (user == null)
                return null;

            string jwtToken = this._jwtService.GenerateJwtToken(user);
            RefreshToken newRefreshToken = GenerateRefreshToken();

            user.RefreshTokens.Add(newRefreshToken);
            this._dbAuthContext.Add(newRefreshToken);

            oldRefreshToken.ReplacedByToken = newRefreshToken.Id;
            oldRefreshToken.Revoked = DateTime.UtcNow;
            oldRefreshToken.ReasonRevoked = "revoked by system";

            await this._dbAuthContext.SaveChangesAsync();

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
    }
}
