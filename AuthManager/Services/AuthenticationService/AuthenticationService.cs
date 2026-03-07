using AuthManager.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthManager.Services.AuthenticationService
{
    public class AuthenticationService([FromServices] IPasswordHasher<User> passwordHasher) : IAuthenticationService
    {
        public bool CheckPassword(User user, string password)
        {
            PasswordVerificationResult result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return result != PasswordVerificationResult.Failed;
        }

        public User CreateUser(string username, string eMail, string password)
        {
            User user = new()
            {
                Username = username,
                EMail = eMail
            };

            user.PasswordHash = passwordHasher.HashPassword(user, password);
            return user;
        }
    }
}
