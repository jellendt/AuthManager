using AuthManager.Contexts;
using AuthManager.Entities;
using AuthManager.Models;
using AuthManager.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AuthManager.Services.AuthenticationService
{
    public class AuthenticationService(
        [FromServices] IUserService userService,
        [FromServices] IJwtService jwtService,
        [FromServices] DbAuthContext dbAuthContext
         ): IAuthenticationService
    {
        private readonly IUserService _userService = userService;
        private readonly IJwtService _jwtService = jwtService;
        private readonly DbAuthContext _dbAuthContext = dbAuthContext;
        public async Task<User> Register(RegisterRequest registerRequest)
        {
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

            user.JwtToken = this._jwtService.GenerateJwtToken(user);

            return user;
        }

        public async Task<User?> Login(LoginRequest loginRequest)
        {
            User? user = await this._userService.GetByEmail(loginRequest.Username);
            if (user == null)
                return null;
            if(PasswordHasher.VerifyPassword(loginRequest.Password, user.PasswordHash, user.Salt))
            {
                user.RefreshTokens.Add(GenerateRefreshToken());
                await this._dbAuthContext.SaveChangesAsync();
                return user;
            }

            return null;
        }

        public async Task<string?> Refresh(string userRefreshToken)
        {
            RefreshToken? oldRefreshToken = await this._dbAuthContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token.Equals(userRefreshToken) && !rt.IsExpired);

            if (oldRefreshToken == null)
                return null;

            RefreshToken newRefreshToken = GenerateRefreshToken();

            oldRefreshToken.ReplacedByToken = newRefreshToken.Id;

            return newRefreshToken.Token;

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
