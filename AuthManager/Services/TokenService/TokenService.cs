using AuthManager.Contexts;
using AuthManager.Entities;
using AuthManager.Exceptions;
using AuthManager.Exceptions.Registration;
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
        public async Task<(string jwtToken, RefreshToken refreshToken)> Register(RegisterRequest registerRequest)
        {
            await CheckUserRequirements(registerRequest);

            User newUser = authenticationService.CreateUser(registerRequest.Username, registerRequest.EMail, registerRequest.Password);
            RefreshToken refreshToken = GenerateRefreshToken();
            newUser.RefreshTokens.Add(refreshToken);
            User user = await userService.Add(newUser);

            return (jwtService.GenerateJwtToken(user), refreshToken);
        }

        public async Task<(string jwtToken, RefreshToken refreshToken)> Login(LoginRequest loginRequest)
        {
            User? user = await userService.GetByEmail(loginRequest.EMail) ?? throw new InvalidCredentialsException();

            if (!authenticationService.CheckPassword(user, loginRequest.Password))
                throw new InvalidCredentialsException();

            RefreshToken newRefreshToken = await UpdateRefreshToken(user);
            string jwtToken = jwtService.GenerateJwtToken(user);
            return (jwtToken, newRefreshToken);

        }

        public async Task<(string jwtToken, RefreshToken refreshToken)> Refresh(string userRefreshToken)
        {
            RefreshToken? oldRefreshToken = await dbAuthContext.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token.Equals(userRefreshToken) && rt.Revoked == null && DateTime.UtcNow < rt.Expires) ?? throw new InvalidCredentialsException();
            User? user = await dbAuthContext.Users.FirstOrDefaultAsync(u => u.Id == oldRefreshToken.UserId) ?? throw new InvalidCredentialsException();

            RefreshToken newRefreshToken = await UpdateRefreshToken(user);
            string jwtToken = jwtService.GenerateJwtToken(user);
            return (jwtToken, newRefreshToken);

        }

        private static void InvalidateOtherRefreshTokens(User user, RefreshToken newRefreshToken)
        {
            foreach (RefreshToken refreshToken in user.RefreshTokens.Where(u => u.IsActive))
            {
                refreshToken.ReplacedByToken = newRefreshToken.Id;
                refreshToken.Expires = DateTime.UtcNow;
                refreshToken.ReasonRevoked = "revoked by system";
            }
        }

        private async Task<RefreshToken> UpdateRefreshToken(User user)
        {
            RefreshToken newRefreshToken = GenerateRefreshToken();
            InvalidateOtherRefreshTokens(user, newRefreshToken);
            user.RefreshTokens.Add(newRefreshToken);
            await dbAuthContext.SaveChangesAsync();
            return newRefreshToken;
        }

        private async Task CheckUserRequirements(RegisterRequest registerRequest)
        {
            if (!EMail().IsMatch(registerRequest.EMail))
                throw new InvalidEMailException();

            if (await userService.CheckEmailExists(registerRequest.EMail))
                throw new EMailInUseException();

            if (await userService.CheckUsernameExists(registerRequest.Username))
                throw new UsernameInUseException();
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
